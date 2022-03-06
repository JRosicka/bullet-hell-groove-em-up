using System;

namespace Rumia {
    /// <summary>
    /// Attribute that, when applied to an <see cref="Emitter"/> field, allows that emitter to be emitted by a
    /// <see cref="RumiaAction"/>.
    ///
    /// This should ONLY be attached to <see cref="Emitter"/>s.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EmissionRumiaActionAttribute : Attribute { }
}