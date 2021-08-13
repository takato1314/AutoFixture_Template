using System.Linq;
using System.Reflection;

namespace AutoFixture.Extensions
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// From MSDN (http://goo.gl/WvOgYq). <br/><br/>
        /// 
        /// To determine if a method is overridable, it is not sufficient to check that IsVirtual is true.
        /// For a method to be overridable, IsVirtual must be true and IsFinal must be false. <br/>
        ///
        /// For example, interface implementations are marked as "virtual final".
        /// Methods marked with "override sealed" are also marked as "virtual final".
        /// </summary>\
        public static bool IsOverridable(this MethodInfo method)
        {
            return method.IsVirtual && !method.IsFinal;
        }

        /// <summary>
        /// Determines if a method is sealed.
        /// </summary>
        public static bool IsSealed(this MethodInfo method)
        {
            return !method.IsOverridable();
        }

        /// <summary>
        /// Determines if a method is void
        /// </summary>
        public static bool IsVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool HasOutParameters(this MethodInfo method)
        {
            return method.GetParameters()
                .Any(p => p.IsOut);
        }

        public static bool HasRefParameters(this MethodInfo method)
        {
            // "out" parameters are also considered "byref", so we have to filter these out
            return method.GetParameters()
                .Any(p => p.ParameterType.IsByRef && !p.IsOut);
        }
    }
}
