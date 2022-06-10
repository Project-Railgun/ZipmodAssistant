using System.ComponentModel;

namespace ZipmodAssistant.Tarot.Utilities
{
  public class DictionaryTypeConverter<TKey, TValue> : CollectionConverter
  {
    public class KeyValuePairDescriptor : PropertyDescriptor
    {
      private readonly KeyValuePair<TKey, TValue> _value;

      public override Type ComponentType => typeof(KeyValuePair<TKey, TValue>);
      public override bool IsReadOnly => true;
      public override Type PropertyType => typeof(TValue);

      public KeyValuePairDescriptor(KeyValuePair<TKey, TValue> value) : base(value.Key.ToString(), null)
      {
        _value = value;
      }

      public override bool CanResetValue(object component) => false;

      public override object? GetValue(object? component) => _value.Value;

      public override void ResetValue(object component) => throw new NotImplementedException();

      public override void SetValue(object? component, object? value) => throw new NotImplementedException();

      public override bool ShouldSerializeValue(object component) => true;
    }
  }
}
