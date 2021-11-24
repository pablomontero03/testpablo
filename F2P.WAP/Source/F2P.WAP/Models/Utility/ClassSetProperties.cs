using System;
using System.Reflection;

namespace F2P.WAP.Models.Utility
{
    public static class ClassSetProperties
    {
        /// <summary>
        /// ivan cortes 
        /// copia las propiedades de un objeto a otro
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyProperties(this Object source, Object destination)
        {
  
            if (source == null || destination == null)
                throw new Exception("Los objetos de origen y / o destino son nulos");

            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }

                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }

    }
}