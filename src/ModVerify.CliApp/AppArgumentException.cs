using System;

namespace AET.ModVerify.App;

internal class AppArgumentException(string message) : ArgumentException(message);