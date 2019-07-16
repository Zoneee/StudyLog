using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



public static class AttributeHelper
{
    /// <summary>
    /// 获取枚举自定义特性信息
    /// </summary>
    /// <typeparam name="T">特性类型</typeparam>
    /// <param name="enum">要获取的枚举项</param>
    /// <returns></returns>
    public static string GetAittributeString<T>(this Enum @enum)
    {
        //获取枚举数据
        Type enumType = @enum.GetType();
        string enumName = Enum.GetName(enumType, @enum);
        T enumAttribute = (T)enumType.GetField(enumName).GetCustomAttributes(false).FirstOrDefault(s => s.GetType().Equals(typeof(T)));
        //获取特性数据
        Type attributeType = typeof(T);
        //PropertyInfo properties = attributeType.GetProperty(aittributeName);
        PropertyInfo properties = attributeType.GetProperties().FirstOrDefault();
        return properties.GetValue(enumAttribute).ToString();
    }
}
