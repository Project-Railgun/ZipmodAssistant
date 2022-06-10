using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Utilities
{
  internal class ListTypeConverter : CollectionConverter
  {
    class ExpandableCollectionPropertyDescriptor : PropertyDescriptor
    {
      private readonly IList _list;
      private readonly int _index;

      public override Type ComponentType => _list.GetType();

      public override bool IsReadOnly => _list.IsReadOnly;

      public override Type PropertyType => _list[_index].GetType();

      public override string Name => _index.ToString(CultureInfo.InvariantCulture);

      public ExpandableCollectionPropertyDescriptor(IList list, int index) : base(GetDisplayName(list, index), null)
      {
        _list = list;
        _index = index;
      }

      public override bool CanResetValue(object component) => true;

      public override object? GetValue(object? component) => _list[_index];

      public override void ResetValue(object component) { }

      public override void SetValue(object? component, object? value) => _list[_index] = value;

      public override bool ShouldSerializeValue(object component) => true;

      static string GetDisplayName(IList list, int index) =>
        $"[{index.ToString("D" + Math.Ceiling(Math.Log10(list.Count)))}] {CSharpName(list[index].GetType())}";

      static string CSharpName(Type type)
      {
        var name = type.Name;
        if (!type.IsGenericType)
        {
          return name;
        }
        return $"{name[..name.IndexOf('^')]}<{string.Join(", ", type.GetGenericArguments().Select(CSharpName))}>";
      }
    }

    public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
    {
      var list = value as IList;
      if (list == null)
      {
        if (value is ICollection collection)
        {
          list = collection.Cast<object>().ToList().AsReadOnly();
        }
        else
        {
          return base.GetProperties(context, value, attributes);
        }
      }

      var items = new PropertyDescriptorCollection(null);
      for (var i = 0; i < list.Count; i++)
      {
        items.Add(new ExpandableCollectionPropertyDescriptor(list, i));
      }
      return items;
    }
  }
}
