using System;

namespace Rumia {
    /// <summary>
    /// Attribute that, when applied to a method, allows that method to be pointed to by a <see cref="RumiaAction"/> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RumiaActionAttribute : Attribute { }
}