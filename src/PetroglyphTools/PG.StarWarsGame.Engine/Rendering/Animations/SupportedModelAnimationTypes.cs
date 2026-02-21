using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Engine.Rendering.Animations;

public static class SupportedModelAnimationTypes
{
    // NB: The games uses two different string mappings for animations.
    // The strings here are used to identify the animation file name.
    // These strings have an underscore '_' as delimiter,
    // while the animation names for the LUA functions (Play_Animation("ANIM NAME")) use a space ' ' as delimiter.

    public static IReadOnlyDictionary<ModelAnimationType, string> GetAnimationTypesForEngine(GameEngineType engineType)
    {
        return engineType switch
        {
            GameEngineType.Eaw => EawSupportedAnimations,
            GameEngineType.Foc => FocSupportedAnimations,
            _ => throw new NotSupportedException()
        };
    }
    
    private static readonly Dictionary<ModelAnimationType, string> EawSupportedAnimations = new()
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
        { ModelAnimationType.PowerUp, "POWERUP"}
    };

    // ReSharper disable StringLiteralTypo
    private static readonly Dictionary<ModelAnimationType, string> FocSupportedAnimations =

        EawSupportedAnimations.Concat(new Dictionary<ModelAnimationType, string>
            {
                { ModelAnimationType.SpinMove, "SPINMOVE" },
                { ModelAnimationType.ForceRevealBegin, "FORCE_REVEAL_BEGIN" },
                { ModelAnimationType.ForceRevealLoop, "FORCE_REVEAL_LOOP" },
                { ModelAnimationType.ForceRevealEnd, "FORCE_REVEAL_END" },
                { ModelAnimationType.SaberThrow, "SWORD_THROW" },
                { ModelAnimationType.SaberControl, "SWORD_CONTROL" },
                { ModelAnimationType.SaberCatch, "SWORD_CATCH" },
                { ModelAnimationType.SaberSpin, "SWORDSPIN" },
                { ModelAnimationType.ContaminateAttack, "CONTAMINATE_ATTACK" },
                { ModelAnimationType.ContaminateLoop, "CONTAMINATE_LOOP" },
                { ModelAnimationType.ContaminateRelease, "CONTAMINATE_RELEASE" },
                { ModelAnimationType.DeployedWalk, "WALK" },
                { ModelAnimationType.PadBuild, "PAD_BUILD" },
                { ModelAnimationType.PadSell, "PAD_SELL" },
                { ModelAnimationType.Heal, "HEAL" },
            })
            .ToDictionary(x => x.Key, x => x.Value);
}