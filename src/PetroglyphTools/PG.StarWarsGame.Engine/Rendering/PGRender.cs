using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.ALO.Files.Animations;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Engine.Rendering;

internal class PGRender(GameRepository gameRepository, GameErrorReporterWrapper errorReporter, IServiceProvider serviceProvider) 
    : IPGRender
{
    private readonly IAloFileService _aloFileService = serviceProvider.GetRequiredService<IAloFileService>();
    private readonly IRepository _modelRepository = gameRepository.ModelRepository;
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(PGRender));

    [SuppressMessage("ReSharper", "StringLiteralTypo")] 
    private static readonly Dictionary<ModelAnimationType, string> AnimationTypeToName = new()
    {
        { ModelAnimationType.Idle, "IDLE"},
        { ModelAnimationType.SpaceIdle, "SPACE_IDLE"},
        { ModelAnimationType.Move, "MOVE"},
        { ModelAnimationType.TurnLeft, "TURNL"},
        { ModelAnimationType.TurnRight, "TURNR"},
        { ModelAnimationType.Attack, "ATTACK"},
        { ModelAnimationType.AttackIdle, "ATTACKIDLE"},
        { ModelAnimationType.Die, "DIE"},
        { ModelAnimationType.Rotate, "ROTATE"},
        { ModelAnimationType.SpecialA, "SPECIAL_A"},
        { ModelAnimationType.SpecialB, "SPECIAL_B"},
        { ModelAnimationType.SpecialC, "SPECIAL_C"},
        { ModelAnimationType.TransitionToLeftTurn, "TURNL_BEGIN"},
        { ModelAnimationType.TransitionFromLeftTurn, "TURNL_END"},
        { ModelAnimationType.TransitionToRightTurn, "TURNR_BEGIN"},
        { ModelAnimationType.TransitionFromRightTurn, "TURNR_END"},
        { ModelAnimationType.TransitionToMove, "MOVESTART"},
        { ModelAnimationType.TransitionFromMoveFrame0, "MOVE_ENDONE"},
        { ModelAnimationType.TransitionFromMoveFrame40, "MOVE_ENDTWO"},
        { ModelAnimationType.TransitionFromMoveFrame80, "MOVE_ENDTHREE"},
        { ModelAnimationType.TransitionFromMoveFrame120, "MOVE_ENDFOUR"},
        { ModelAnimationType.TurnLeftHalf, "TURNL_HALF"},
        { ModelAnimationType.TurnLeftQuarter, "TURNL_QUARTER"},
        { ModelAnimationType.TurnRightHalf, "TURNR_HALF"},
        { ModelAnimationType.TurnRightQuarter, "TURNR_QUARTER"},
        { ModelAnimationType.Deploy, "DEPLOY"},
        { ModelAnimationType.Undeploy, "UNDEPLOY"},
        { ModelAnimationType.Cinematic, "CINEMATIC"},
        { ModelAnimationType.BlockBlaster, "BLOCK_BLASTER"},
        { ModelAnimationType.RedirectBlaster, "REDIRECT_BLASTER"},
        { ModelAnimationType.IdleBlockBlaster, "IDLE_BLOCKBLASTER"},
        { ModelAnimationType.ForceWhirlwindAttack, "FW_ATTACK"},
        { ModelAnimationType.ForceWhirlwindDie, "FW_DIE"},
        { ModelAnimationType.ForceTelekinesisAttack, "FTK_ATTACK"},
        { ModelAnimationType.ForceTelekinesisHold, "FTK_HOLD"},
        { ModelAnimationType.ForceTelekinesisRelease, "FTK_RELEASE"},
        { ModelAnimationType.ForceTelekinesisDie, "FTK_DIE"},
        { ModelAnimationType.EarthquakeAttack, "FB_ATTACK"},
        { ModelAnimationType.EarthquakeHold, "FB_HOLD"},
        { ModelAnimationType.EarthquakeRelease, "FB_RELEASE"},
        { ModelAnimationType.ForceLightningAttack, "FL_ATTACK"},
        { ModelAnimationType.ForceLightningDie, "FL_DIE"},
        { ModelAnimationType.ForceRun, "FORCE_RUN"},
        { ModelAnimationType.TransportLanding, "LAND"},
        { ModelAnimationType.TransportLeaving, "TAKEOFF"},
        { ModelAnimationType.FlameAttack, "FLAME_ATTACK"},
        { ModelAnimationType.Demolition, "DEMOLITION"},
        { ModelAnimationType.BombToss, "BOMBTOSS"},
        { ModelAnimationType.Jump, "JUMP"},
        { ModelAnimationType.FlyIdle, "FLYIDLE"},
        { ModelAnimationType.FlyLand, "FLYLAND"},
        { ModelAnimationType.LandIdle, "FLYLANDIDLE"},
        { ModelAnimationType.Land, "FLYLANDDROP"},
        { ModelAnimationType.HcWin, "HC_WIN"},
        { ModelAnimationType.HcLose, "HC_LOSE"},
        { ModelAnimationType.HcDraw, "HC_DRAW"},
        { ModelAnimationType.ShieldOn, "SHIELD_ON"},
        { ModelAnimationType.ShieldOff, "SHIELD_OFF"},
        { ModelAnimationType.CableAttackDie, "CA_DIE"},
        { ModelAnimationType.DeployedCableAttackDie, "DEPLOYED_CA_DIE"},
        { ModelAnimationType.DeployedDie, "DEPLOYED_DIE"},
        { ModelAnimationType.RunAroundOnFire, "FIRE_MOVE"},
        { ModelAnimationType.FireDie, "FIRE_DIE"},
        { ModelAnimationType.PoundAttack, "POUND_ATTACK"},
        { ModelAnimationType.EatAttack, "EAT_ATTACK"},
        { ModelAnimationType.EatDie, "EATEN_DIE"},
        { ModelAnimationType.MoveWalk, "WALKMOVE"},
        { ModelAnimationType.MoveCrouch, "CROUCHMOVE"},
        { ModelAnimationType.StructureOpen, "OPEN"},
        { ModelAnimationType.StructureHold, "HOLD"},
        { ModelAnimationType.StructureClose, "CLOSE"},
        { ModelAnimationType.IdleCrouch, "CROUCHIDLE"},
        { ModelAnimationType.TurnLeftCrouch, "CROUCHTURNL"},
        { ModelAnimationType.TurnRightCrouch, "CROUCHTURNR"},
        { ModelAnimationType.Build, "BUILD"},
        { ModelAnimationType.TransitionOwnership, "TRANS"},
        { ModelAnimationType.SelfDestruct, "SELF_DESTRUCT"},
        { ModelAnimationType.Attention, "ATTENTION"},
        { ModelAnimationType.Celebrate, "CELEBRATE"},
        { ModelAnimationType.FlinchLeft, "FLINCHL"},
        { ModelAnimationType.FlinchRight, "FLINCHR"},
        { ModelAnimationType.FlinchFront, "FLINCHF"},
        { ModelAnimationType.FlinchBack, "FLINCHB"},
        { ModelAnimationType.AttackFlinchLeft, "ATTACKFLINCHL"},
        { ModelAnimationType.AttackFlinchRight, "ATTACKFLINCHR"},
        { ModelAnimationType.AttackFlinchFront, "ATTACKFLINCHF"},
        { ModelAnimationType.AttackFlinchBack, "ATTACKFLINCHB"},
        { ModelAnimationType.Talk, "TALK"},
        { ModelAnimationType.TalkGesture, "TALKGESTURE"},
        { ModelAnimationType.TalkQuestion, "TALKQUESTION"},
        { ModelAnimationType.Hacking, "HACKING"},
        { ModelAnimationType.Repairing, "REPAIRING"},
        { ModelAnimationType.Choke, "CHOKE"},
        { ModelAnimationType.ChokeDie, "CHOKEDEATH"},
        { ModelAnimationType.DropTroopers, "TROOPDROP"},
        { ModelAnimationType.RopeSlide, "ROPESLIDE"},
        { ModelAnimationType.RopeLand, "ROPELAND"},
        { ModelAnimationType.RopeDrop, "ROPE_DROP"},
        { ModelAnimationType.RopeLift, "ROPE_LIFT"},
        { ModelAnimationType.Alarm, "ALARM"},
        { ModelAnimationType.Warning, "WARNING"},
        { ModelAnimationType.Crushed, "CRUSHED"},
        { ModelAnimationType.PowerDown, "POWERDOWN"},
        { ModelAnimationType.PowerUp, "POWERUP"},
        { ModelAnimationType.SpinMove, "SPINMOVE"},
        { ModelAnimationType.ForceRevealBegin, "FORCE_REVEAL_BEGIN"},
        { ModelAnimationType.ForceRevealLoop, "FORCE_REVEAL_LOOP"},
        { ModelAnimationType.ForceRevealEnd, "FORCE_REVEAL_END"},
        { ModelAnimationType.SaberThrow, "SWORD_THROW"},
        { ModelAnimationType.SaberControl, "SWORD_CONTROL"},
        { ModelAnimationType.SaberCatch, "SWORD_CATCH"},
        { ModelAnimationType.SaberSpin, "SWORDSPIN"},
        { ModelAnimationType.ContaminateAttack, "CONTAMINATE_ATTACK"},
        { ModelAnimationType.ContaminateLoop, "CONTAMINATE_LOOP"},
        { ModelAnimationType.ContaminateRelease, "CONTAMINATE_RELEASE"},
        { ModelAnimationType.DeployedWalk, "WALK"},
        { ModelAnimationType.PadBuild, "PAD_BUILD"},
        { ModelAnimationType.PadSell, "PAD_SELL"},
        { ModelAnimationType.Heal, "HEAL"},
    };

    public IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(string path, bool metadataOnly = true)
    {
        return Load3DAsset(path.AsSpan(), metadataOnly);
    }

    public IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(ReadOnlySpan<char> path, bool metadataOnly = true)
    {
        if (path.IsEmpty)
            errorReporter.Assert(EngineAssert.FromNullOrEmpty(null, "Model path is null or empty."));

        using var aloStream = _modelRepository.TryOpenFile(path);
        if (aloStream is null)
            return null;

        var loadOptions = metadataOnly ? AloLoadOptions.MetadataOnly : AloLoadOptions.Full;

        try
        {
            return _aloFileService.Load(aloStream, loadOptions);
        }
        catch (BinaryCorruptedException e)
        {
            var pathString = path.ToString();
            var errorMessage = $"Unable to load 3D asset '{pathString}': {e.Message}";
            _logger?.LogWarning(e, errorMessage);
            errorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, pathString, null, errorMessage));
            return null;
        }
    }

    public ModelClass? LoadModelAndAnimations(ReadOnlySpan<char> path, string? animOverrideName, bool metadataOnly = true)
    {
        var aloFile = Load3DAsset(path, metadataOnly);
        
        if (aloFile is null)
            return null;

        var modelClass = new ModelClass(aloFile);

        if (!modelClass.IsModel)
            return modelClass;

        var dir = _fileSystem.Path.GetDirectoryName(path);
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(path);

        if (!string.IsNullOrEmpty(animOverrideName))
            fileName = _fileSystem.Path.GetFileNameWithoutExtension(animOverrideName.AsSpan());

        Span<char> stringBuffer = stackalloc char[256];
        
        foreach (ModelAnimationType animType in Enum.GetValues(typeof(ModelAnimationType)))
        {
            var subIndex = 0;
            var loadingNumberedAnimations = true;

            var animName = AnimationTypeToName[animType];

            while (loadingNumberedAnimations)
            {
                var stringBuilder = new ValueStringBuilder(stringBuffer);
                
                CreateAnimationFilePath(ref stringBuilder, fileName, animName, subIndex);
                var animationFilenameWithoutExtension =
                    _fileSystem.Path.GetFileNameWithoutExtension(stringBuilder.AsSpan());
                InsertPath(ref stringBuilder, dir);

                if (stringBuilder.Length > PGConstants.MaxAnimationFileName)
                {
                    var animFile = stringBuilder.AsSpan().ToString();
                    var model = path.ToString();
                    errorReporter.Assert(
                        EngineAssert.Create(EngineAssertKind.ValueOutOfRange, animFile, model,
                            $"Cannot get animation file '{animFile}' for model '{model}', because animation file path is too long."));
                    return null;
                }
                var animationAsset = Load3DAsset(stringBuilder.AsSpan(), metadataOnly);
                if (animationAsset is IAloAnimationFile animationFile)
                {
                    loadingNumberedAnimations = true;
                    var crc = _hashingService.GetCrc32(animationFilenameWithoutExtension, PGConstants.DefaultPGEncoding);
                    modelClass.Animations.AddAnimation(animType, animationFile, crc);
                }
                else
                {
                    loadingNumberedAnimations = false;
                }

                stringBuilder.Dispose();
                subIndex++;
            }
        }
        return modelClass;
    }

    private void InsertPath(ref ValueStringBuilder stringBuilder, ReadOnlySpan<char> directory)
    {
        if (!_fileSystem.Path.HasTrailingDirectorySeparator(directory))
            stringBuilder.Insert(0, '\\', 1);
        stringBuilder.Insert(0, directory);
    }

    private static void CreateAnimationFilePath(
        ref ValueStringBuilder stringBuilder,
        ReadOnlySpan<char> fileName, 
        string animationTypeName, 
        int subIndex)
    {
        stringBuilder.Append(fileName);
        stringBuilder.Append('_');
        stringBuilder.Append(animationTypeName);
        stringBuilder.Append('_');
#if NETSTANDARD2_0 || NETFRAMEWORK
        stringBuilder.Append(subIndex.ToString("D2"));
#else
        subIndex.TryFormat(stringBuilder.AppendSpan(2), out var n, "D2");
#endif
        stringBuilder.Append(".ALA");
    }
}