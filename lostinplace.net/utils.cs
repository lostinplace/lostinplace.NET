using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Script.Serialization;
using System.Reflection;

namespace lostinplace.net
{
  /// <summary>
  /// Dictionary child that implements json dictionary format 
  /// </summary>
  /// <example>aDict[invalidKey]==null, (aDict[invalidKey]=value)==(aDict.add(invalidKey,value)) </example>
  /// <typeparam name="TKey">Type of dictionary index objects</typeparam>
  /// <typeparam name="TValue">Type of dictionary value objects</typeparam>
  public class eDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  {
    public TValue this[TKey index]
    {
      get
      {
        return this.ContainsKey(index) ? base[index] : default(TValue);
      }
      set
      {
        if (this.ContainsKey(index)) base[index] = value;
        else base.Add(index, value);
      }
    }

    public void Add(TKey anIndex, TValue aValue)
    {
      if (this.ContainsKey(anIndex)) base[anIndex] = aValue;
      else base.Add(anIndex, aValue);
    }
  }

  public class CustomJavaScriptConverter : JavaScriptConverter
  {
    public static JavaScriptSerializer BuildSerializer(IEnumerable<Type> SupportedTypesList, Func<PropertyInfo, bool> FormatterFunction)
    {
      JavaScriptSerializer tmpSerializer = new JavaScriptSerializer();
      CustomJavaScriptConverter tmpConverter = new CustomJavaScriptConverter(FormatterFunction);
      tmpConverter.SupportedTypesList = SupportedTypesList;
      tmpSerializer.RegisterConverters(new List<JavaScriptConverter>() { tmpConverter });
      return tmpSerializer;
    }

    public override IEnumerable<Type> SupportedTypes
    {
      get { return SupportedTypesList; }
    }

    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    {
      object tmpObject = type.GetConstructor(null).Invoke(null);
      PropertyInfo tmpInfo;
      foreach (string item in dictionary.Keys)
      {
        tmpInfo = type.GetProperty(item);
        if (tmpInfo.PropertyType.IsValueType) tmpInfo.SetValue(tmpObject, serializer.ConvertToType(dictionary[item], tmpInfo.PropertyType), null);
      }
      return tmpObject;
    }

    public Func<PropertyInfo, bool> FormatterFunction = (x => true);

    public IEnumerable<Type> SupportedTypesList;

    public CustomJavaScriptConverter(Func<PropertyInfo, bool> aFormatterFunction)
      : base()
    {

      FormatterFunction = aFormatterFunction;
    }

    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    {
      Type objType = obj.GetType();
      IDictionary<string, object> serialized = new Dictionary<string, object>();
      foreach (PropertyInfo item in objType.GetProperties())
      {
        if (item.PropertyType.Assembly.ManifestModule.Name != "System.Data.Entity.dll")
          serialized[item.Name] = item.GetValue(obj, null);
      }
      //serialized["Name"] = p.Name;
      //serialized["Birthday"] = p.Birthday.ToString(_dateFormat);
      return serialized;
    }
  }

  public class FakeHttpRequestWrapper : HttpRequestWrapper
  {
    public System.Collections.Specialized.NameValueCollection fldParams;
    public string fldPath;

    public override string Path
    {
      get
      {
        return fldPath;
      }
    }

    public override System.Collections.Specialized.NameValueCollection Params
    {
      get
      {
        return fldParams;
      }
    }


    public FakeHttpRequestWrapper(HttpRequest aRequest)
      : base(aRequest)
    {
      this.fldPath = aRequest.Path;
      this.fldParams = new System.Collections.Specialized.NameValueCollection();
      int i = 0;
      foreach (string item in aRequest.Params)
      {
        fldParams.Add(aRequest.Params.AllKeys.ElementAt(i++), aRequest.Params[item]);
      }
    }
  }
}
