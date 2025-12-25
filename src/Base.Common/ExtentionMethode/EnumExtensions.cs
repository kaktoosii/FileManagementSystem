using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Base.Common;

/// <summary>
/// برای مدیریت اینام ها
/// </summary>
public static class EnumExtensions
{
    #region GetDisplayName
    /// <summary>
    /// در یافت نام فارسی اینام
    /// </summary>
    /// <param name="enu"></param>
    /// <returns></returns>
    public static string GetDisplayName(this Enum enu)
    {
        if (enu == null)
            return null;
        var attr = GetDisplayAttribute(enu);
        return attr != null ? attr.Name : enu.ToString();
    }
    #endregion
    public static string GetEnumDescription(Enum value)
    {
        return value.GetDisplayName();
    }

    #region GetDescription
    /// <summary>
    /// دریافت توضیح فارسی اینام
    /// </summary>
    /// <param name="enu"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum enu)
    {
        if (enu == null)
            return null;
        var attr = GetDisplayAttribute(enu);
        return attr != null ? attr.Description : enu.ToString();
    }
    #endregion

    #region GetDisplayAttribute
    /// <summary>
    /// متد دریافت اتربیوت اینام
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static DisplayAttribute GetDisplayAttribute(object value)
    {

        Type type = value.GetType();
        var field = type.GetField(value.ToString());
        //return field == null ? null : field.GetCustomAttributes(typeof(DisplayAttribute), true);
        if (field == null)
        {
            return null;
        }
        else
        {
            Object[] displayAttributes = field.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (displayAttributes != null && displayAttributes.Length == 1)
            {
                return ((DisplayAttribute)displayAttributes[0]);
            }

            return null;
        }
    }
    #endregion

    private static string GetDisplayName<TEnum>(TEnum @enum) where TEnum : struct, IConvertible
    {
        return @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?
                    .GetCustomAttribute<DisplayAttribute>(false)?.Name ?? @enum.ToString();
    }

    public static string EmptyIfNull(this object value)
    {
        if (value == null)
        {
            return "";
        }

        return value.ToString();
    }

    public static T GetEnumValueFromDisplayName<T>(string displayName)
    {
        var enumType = typeof(T);
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        var enumValues = Enum.GetValues(enumType).Cast<T>();
        foreach (var value in enumValues)
        {
            var field = enumType.GetField(value.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            if (displayAttribute != null && displayAttribute.Name.Contains(displayName, StringComparison.Ordinal))
            {
                return value;
            }
        }

        throw new ArgumentException($"No enum value found with the display name '{displayName}'");
    }

}