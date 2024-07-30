using System.Reflection.Emit;

namespace ECSLib.XML.Extensions;

internal static class CodeGeneratorExtension
{
    public static void EmitLoadConstant(this ILGenerator il, object value)
    {
        switch (value)
        {
            case int i:
                il.Emit(OpCodes.Ldc_I4, i);
                break;
            case char c:
                il.Emit(OpCodes.Ldc_I4, c);
                break;    
            case float f:
                il.Emit(OpCodes.Ldc_R4, f);
                break;
            case double d:
                il.Emit(OpCodes.Ldc_R8, d);
                break;
            case bool b:
                il.Emit(OpCodes.Ldc_R4, b ? 1 : 0);
                break;
            case short s:
                il.Emit(OpCodes.Ldc_I4_S, s);
                break;
            case long l:
                il.Emit(OpCodes.Ldc_I8, l);
                break;
            case uint ui:
                il.Emit(OpCodes.Ldc_I4, ui);
                break;
            case ushort us:
                il.Emit(OpCodes.Ldc_I4_S, us);
                break;
            case ulong ul:
                il.Emit(OpCodes.Ldc_I8, ul);
                break;
            case byte by:
                il.Emit(OpCodes.Ldc_I4_S, by);
                break;
            default:
                throw new ArgumentException("Unsupported type " + value.GetType().FullName + " for loading as constant in IL code.");
        }
    }
}