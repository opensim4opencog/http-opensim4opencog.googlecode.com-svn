/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public enum OSDType
    {
        /// <summary></summary>
        Unknown,
        /// <summary></summary>
        Boolean,
        /// <summary></summary>
        Integer,
        /// <summary></summary>
        Real,
        /// <summary></summary>
        String,
        /// <summary></summary>
        UUID,
        /// <summary></summary>
        Date,
        /// <summary></summary>
        URI,
        /// <summary></summary>
        Binary,
        /// <summary></summary>
        Map,
        /// <summary></summary>
        Array
    }

    public enum OSDFormat
    {
        Xml = 0,
        Json,
        Binary
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDException : Exception
    {
        public OSDException(string message) : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class OSD
    {
        public virtual OSDType Type { get { return OSDType.Unknown; } }
        public static void IsMissing()
        {
            throw new NotImplementedException();
        }

        public virtual bool AsBoolean() { return false; }
        public virtual int AsInteger() { IsMissing(); return 0; }
        public virtual int AsIntegerD() { return 0; }
        public virtual uint AsUInteger() { IsMissing(); return 0; }
        public virtual long AsLong() { IsMissing(); return 0; }
        public virtual ulong AsULong() { IsMissing(); return 0; }
        public virtual double AsReal() { IsMissing(); return 0d; }
        public virtual string AsString() { return String.Empty; }
        public virtual UUID AsUUID()
        {
            IsMissing();
            return UUID.Zero;
        }

        public virtual DateTime AsDate() { IsMissing(); return Utils.Epoch; }
        public virtual Uri AsUri() { IsMissing(); return null; }
        public virtual byte[] AsBinary() { IsMissing(); return Utils.EmptyBytes; }
        public virtual Vector2 AsVector2() { IsMissing(); return Vector2.Zero; }
        public virtual Vector3 AsVector3() { IsMissing(); return Vector3.Zero; }
        public virtual Vector3d AsVector3d() { IsMissing(); return Vector3d.Zero; }
        public virtual Vector4 AsVector4() { IsMissing(); return Vector4.Zero; }
        public virtual Quaternion AsQuaternion() { IsMissing(); return Quaternion.Identity; }
        public virtual Color4 AsColor4() { IsMissing(); return Color4.Black; }

        public override string ToString() { return "undef"; }

        public static OSD FromBoolean(bool value) { return new OSDBoolean(value); }
        public static OSD FromInteger(int value) { return new OSDInteger(value); }
        public static OSD FromInteger(uint value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(short value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(ushort value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(sbyte value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(byte value) { return new OSDInteger((int)value); }
        public static OSD FromUInteger(uint value) { return new OSDBinary(value); }
        public static OSD FromLong(long value) { return new OSDBinary(value); }
        public static OSD FromULong(ulong value) { return new OSDBinary(value); }
        public static OSD FromReal(double value) { return new OSDReal(value); }
        public static OSD FromReal(float value) { return new OSDReal((double)value); }
        public static OSD FromString(string value) { return new OSDString(value); }
        public static OSD FromUUID(UUID value) { return new OSDUUID(value); }
        public static OSD FromDate(DateTime value) { return new OSDDate(value); }
        public static OSD FromUri(Uri value) { return new OSDUri(value); }
        public static OSD FromBinary(byte[] value) { return new OSDBinary(value); }

        public static OSD FromVector2(Vector2 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            return array;
        }

        public static OSD FromVector3(Vector3 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector3d(Vector3d value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector4(Vector4 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromQuaternion(Quaternion value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromColor4(Color4 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.R));
            array.Add(OSD.FromReal(value.G));
            array.Add(OSD.FromReal(value.B));
            array.Add(OSD.FromReal(value.A));
            return array;
        }
         public static OSD FromObject(object value)
         {
             var osd = FromObject0(value);
             if (osd.Type == OSDType.Unknown) IsMissing();
             return osd;
         }
        public static OSD FromObject0(object value)
        {
            if (value == null) { return new OSD(); }
            else if (value is bool) { return new OSDBoolean((bool)value); }
            else if (value is int) { return new OSDInteger((int)value); }
            else if (value is uint) { return new OSDBinary((uint)value); }
            else if (value is short) { return new OSDInteger((int)(short)value); }
            else if (value is ushort) { return new OSDInteger((int)(ushort)value); }
            else if (value is sbyte) { return new OSDInteger((int)(sbyte)value); }
            else if (value is byte) { return new OSDInteger((int)(byte)value); }
            else if (value is double) { return new OSDReal((double)value); }
            else if (value is float) { return new OSDReal((double)(float)value); }
            else if (value is string) { return new OSDString((string)value); }
            else if (value is UUID) { return new OSDUUID((UUID)value); }
            else if (value is DateTime) { return new OSDDate((DateTime)value); }
            else if (value is Uri) { return new OSDUri((Uri)value); }
            else if (value is byte[]) { return new OSDBinary((byte[])value); }
            else if (value is long) { return new OSDBinary((long)value); }
            else if (value is ulong) { return new OSDBinary((ulong)value); }
            else if (value is Vector2) { return FromVector2((Vector2)value); }
            else if (value is Vector3) { return FromVector3((Vector3)value); }
            else if (value is Vector3d) { return FromVector3d((Vector3d)value); }
            else if (value is Vector4) { return FromVector4((Vector4)value); }
            else if (value is Quaternion) { return FromQuaternion((Quaternion)value); }
            else if (value is Color4) { return FromColor4((Color4)value); }
            else return new OSD();
        }
        public static object ToObject(Type type, OSD value)
        {
            var o = ToObject0(type, value);
            if (o == null)
            {
                IsMissing();
            }
            return o;
        }

        public static object ToObject0(Type type, OSD value)
        {
            if (type == typeof(UUID[]))
            {
                if (value.Type == OSDType.Array)
                {
                    var osda = (OSDArray) value;
                    int len = osda.Count;
                    var o = new UUID[len];
                    for (int i = 0; i < osda.Count; i++)
                    {
                        OSD osd = osda[i];
                        o[i] = osd.AsUUID();
                    }
                    return o;
                }
                if (value.Type == OSDType.Unknown)
                {
                    return null;
                }
                IsMissing();
            }
            if (type == typeof(ulong))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt64(bytes);
                }
                else
                {
                    return (ulong)value.AsInteger();
                }
            }
            else if (type == typeof(uint))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt(bytes);
                }
                else
                {
                    return (uint)value.AsInteger();
                }
            }
            else if (type == typeof(ushort))
            {
                return (ushort)value.AsInteger();
            }
            else if (type == typeof(byte))
            {
                return (byte)value.AsInteger();
            }
            else if (type == typeof(short))
            {
                return (short)value.AsInteger();
            }
            else if (type == typeof(string))
            {
                return value.AsString();
            }
            else if (type == typeof(bool))
            {
                return value.AsBoolean();
            }
            else if (type == typeof(float))
            {
                return (float)value.AsReal();
            }
            else if (type == typeof(double))
            {
                return value.AsReal();
            }
            else if (type == typeof(int))
            {
                return value.AsInteger();
            }
            else if (type == typeof(UUID))
            {
                return value.AsUUID();
            }
            else if (type == typeof(Vector3))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector3();
                else
                    return Vector3.Zero;
            }
            else if (type == typeof(Vector4))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector4();
                else
                    return Vector4.Zero;
            }
            else if (type == typeof(Quaternion))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsQuaternion();
                else
                    return Quaternion.Identity;
            }
            else
            {
                return null;
            }
        }

        #region Implicit Conversions

        public static implicit operator OSD(bool value) { return new OSDBoolean(value); }
        public static implicit operator OSD(int value) { return new OSDInteger(value); }
        public static implicit operator OSD(uint value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(short value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(ushort value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(sbyte value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(byte value) { return new OSDInteger((int)value); }
        public static implicit operator OSD(long value) { return new OSDBinary(value); }
        public static implicit operator OSD(ulong value) { return new OSDBinary(value); }
        public static implicit operator OSD(double value) { return new OSDReal(value); }
        public static implicit operator OSD(float value) { return new OSDReal(value); }
        public static implicit operator OSD(string value) { return new OSDString(value); }
        public static implicit operator OSD(UUID value) { return new OSDUUID(value); }
        public static implicit operator OSD(DateTime value) { return new OSDDate(value); }
        public static implicit operator OSD(Uri value) { return new OSDUri(value); }
        public static implicit operator OSD(byte[] value) { return new OSDBinary(value); }
        public static implicit operator OSD(Vector2 value) { return OSD.FromVector2(value); }
        public static implicit operator OSD(Vector3 value) { return OSD.FromVector3(value); }
        public static implicit operator OSD(Vector3d value) { return OSD.FromVector3d(value); }
        public static implicit operator OSD(Vector4 value) { return OSD.FromVector4(value); }
        public static implicit operator OSD(Quaternion value) { return OSD.FromQuaternion(value); }
        public static implicit operator OSD(Color4 value) { return OSD.FromColor4(value); }

        public static implicit operator bool(OSD value) { return value.AsBoolean(); }
        public static implicit operator int(OSD value) { return value.AsInteger(); }
        public static implicit operator uint(OSD value) { return value.AsUInteger(); }
        public static implicit operator long(OSD value) { return value.AsLong(); }
        public static implicit operator ulong(OSD value) { return value.AsULong(); }
        public static implicit operator double(OSD value) { return value.AsReal(); }
        public static implicit operator float(OSD value) { return (float)value.AsReal(); }
        public static implicit operator string(OSD value) { return value.AsString(); }
        public static implicit operator UUID(OSD value) { return value.AsUUID(); }
        public static implicit operator DateTime(OSD value) { return value.AsDate(); }
        public static implicit operator Uri(OSD value) { return value.AsUri(); }
        public static implicit operator byte[](OSD value) { return value.AsBinary(); }
        public static implicit operator Vector2(OSD value) { return value.AsVector2(); }
        public static implicit operator Vector3(OSD value) { return value.AsVector3(); }
        public static implicit operator Vector3d(OSD value) { return value.AsVector3d(); }
        public static implicit operator Vector4(OSD value) { return value.AsVector4(); }
        public static implicit operator Quaternion(OSD value) { return value.AsQuaternion(); }
        public static implicit operator Color4(OSD value) { return value.AsColor4(); }

        #endregion Implicit Conversions

        /// <summary>
        /// Uses reflection to create an SDMap from all of the SD
        /// serializable types in an object
        /// </summary>
        /// <param name="obj">Class or struct containing serializable types</param>
        /// <returns>An SDMap holding the serialized values from the
        /// container object</returns>
        public static OSDMap SerializeMembers(object obj)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            OSDMap map = new OSDMap(fields.Length);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField = OSD.FromObject(field.GetValue(obj));

                    if (serializedField.Type != OSDType.Unknown || field.FieldType == typeof(string) || field.FieldType == typeof(byte[]))
                        map.Add(field.Name, serializedField);
                }
            }

            return map;
        }

        /// <summary>
        /// Uses reflection to deserialize member variables in an object from
        /// an SDMap
        /// </summary>
        /// <param name="obj">Reference to an object to fill with deserialized
        /// values</param>
        /// <param name="serialized">Serialized values to put in the target
        /// object</param>
        public static void DeserializeMembers(ref object obj, OSDMap serialized)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField;
                    if (serialized.TryGetValue(field.Name, out serializedField))
                        field.SetValue(obj, ToObject(field.FieldType, serializedField));
                }
            }
        }


        public static void AddObjectOSD(object primitive, OSDMap map, Type from)
        {
            from = from ?? primitive.GetType();
            map.Add("typeosd", from.FullName);
            foreach (var v in from.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                string n = v.Name;
                if (n.StartsWith("_")) continue;
                if (v is MethodBase)
                {
                    continue;
                }
                if (v.IsDefined(typeof(NonSerializedAttribute), true)) continue;
                var p = v as PropertyInfo;
                if (p != null && p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                {
                    n = "p_" + n;
                    AddOSDMember(map, n, p.GetValue(primitive, null), p.PropertyType);
                    continue;
                }
                var f = v as FieldInfo;
                if (f != null && !f.IsStatic && !f.IsLiteral)
                {
                    n = "f_" + n;
                    AddOSDMember(map, n, f.GetValue(primitive), f.FieldType);
                    continue;
                }
            }
        }
        public static void SetObjectOSD(object primitive, OSDMap map)
        {
            Type from = primitive.GetType();
            string From = map["typeosd"].AsString();//, primitive.GetType().FullName);
            from = System.Type.GetType(From) ?? from;
            int mapManips = 0;
            foreach (var v in from.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (v is MethodBase)
                {
                    continue;
                }
                if (v.IsDefined(typeof(NonSerializedAttribute), true)) continue;
                string n = v.Name;
                if (n.StartsWith("_")) continue;
                var p = v as PropertyInfo;
                if (p != null && p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                {
                    bool found;
                    n = "p_" + n;
                    var suggest = p.GetValue(primitive, null);
                    object setOSDMember = GetOSDMember(suggest,map, n, p.PropertyType, out found);
                    if (found)
                    {
                        p.SetValue(primitive, setOSDMember, null);
                        mapManips++;
                        continue;
                    }
                    continue;
                }
                var f = v as FieldInfo;
                if (f != null && !f.IsStatic && !f.IsLiteral)
                {
                    bool found;
                    n = "f_" + n;
                    var suggest = f.GetValue(primitive);
                    object setOSDMember = GetOSDMember(suggest, map, n, f.FieldType, out found);
                    if (found)
                    {
                        f.SetValue(primitive, setOSDMember);
                        mapManips++;
                        continue;
                    }
                    continue;
                }
            }
        }

        private static object GetOSDMember(object suggest, OSDMap map, string s, Type type, out bool found)
        {
            var v = map[s];
            if (v == null)
            {
                found = false;
                return null;
            }
            found = true;
            if (type == typeof(string)) return v.AsString();
            if (type == typeof(Vector3)) return v.AsVector3();
            if (type == typeof(UUID)) return v.AsUUID();
            var oo = ToObject0(type, v);
            if (oo!=null)
            {
                return oo;
            }
            found = false;
            oo = ConvertOP(v, new[] { typeof(OSD) }, type, typeof(OSD), out found);
            if (found)
            {
                return oo;
            }
            oo = ConvertOP(v, new[] { typeof(OSD) }, type, type, out found);
            if (found)
            {
                return oo;
            }
            if (!(v is OSDMap))
            {
                return oo;
            }
            map = v as OSDMap;
            if (suggest != null)
            {
                SetObjectOSD(suggest,map);
                found = true;
                return suggest;
            }
            if (map["typeosd"])
            {
                string sntype = map["typeosd"];
                System.Type ntype = System.Type.GetType(sntype);
                if (ntype == null)
                {
                    if (!type.IsAbstract && !type.IsInterface)
                        ntype = type;
                }
                if (ntype != null && type.IsAssignableFrom(ntype))
                {
                    var ci = ntype.GetConstructor(new Type[0]);
                    if (ci != null)
                    {
                        oo = ci.Invoke(new object[0]);
                        SetObjectOSD(oo, map);
                        found = true;
                    }
                    else
                    {
                        ci = ntype.GetConstructor(new Type[] { typeof(OSD) });
                        if (ci != null)
                        {
                            oo = ci.Invoke(new object[] { v });
                            found = true;
                        }
                    }
                }
            }
            return oo;
            //return v;
        }

        private const BindingFlags basePropertyFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public static object ConvertOP(object StartObject, Type[] fromTypes, Type toType, Type operatorType, out bool found)
        {
            found = true;
            toType = toType ?? operatorType;
            object retval = null;
            if (fromTypes == null)
                fromTypes = (StartObject != null) ? new Type[] { StartObject.GetType() } : new Type[0];

            MethodInfo mi = null;
            try
            {
                mi = operatorType.GetMethod("op_Explicit", (BindingFlags.Public | BindingFlags.Static), null,
                                                  fromTypes, new ParameterModifier[0]);
            }
            catch (AmbiguousMatchException)
            {
                mi = null;
            }

            if (mi != null && !toType.IsAssignableFrom(mi.ReturnType)) mi = null;

            try
            {
                if (mi == null)
                    mi = operatorType.GetMethod("op_Implicit", (BindingFlags.Public | BindingFlags.Static), null, fromTypes,
                                           new ParameterModifier[0]);
            }
            catch (AmbiguousMatchException)
            {
                mi = null;
            }

            if (mi != null && !toType.IsAssignableFrom(mi.ReturnType)) mi = null;

            var objects = new object[] { StartObject };
            if (mi != null) //there is a conversion operator!
            {
                try
                {
                    return operatorType.InvokeMember(mi.Name,
                                                  BindingFlags.InvokeMethod | (BindingFlags.Public | BindingFlags.Static),
                                                  null, null, objects);
                }
                catch (AmbiguousMatchException)
                {
                    return mi.Invoke(null, objects);
                }
            }
            foreach (var m in operatorType.GetMethods(BindingFlags.InvokeMethod | (BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)))
            {
                Type returnType = m.ReturnType;
                // check if too general
                if (returnType == typeof(object) && returnType != toType) continue;
                if (!toType.IsAssignableFrom(returnType)) continue;
                ParameterInfo[] pts = m.GetParameters();
                if (pts.Length != 1) continue;
                foreach (var searchType in fromTypes)
                {
                    Type paramType = pts[0].ParameterType;
                    if (paramType == typeof(object))
                    {
                        //too general
                        continue;
                    }
                    if (paramType.IsAssignableFrom(searchType))
                    {
                        return m.Invoke(null, objects);
                    }
                }
            }

            found = false;
            return retval;
        }
        private static void AddOSDMember(OSDMap map, string s, object value, Type type)
        {
            if (type == typeof(UUID))
            {
                var uuid = (UUID)value;
                if (!UUIDFactory.IsNullOrZero(uuid))
                {
                    map[s] = uuid;
                    return;
                }
            }
            if (map.ContainsKey(s)) return;
            bool added = AddOSDMember0(map, s, value, type);
            if (added) return;
            if (value == null) return;
            MethodInfo toOSD = type.GetMethod("GetOSD");
            if (toOSD != null)
            {
                var osdv = toOSD.Invoke(value, new object[0]) as OSD;
                if (osdv != null)
                {
                    map.Add(s, osdv);
                    return;
                }
            }
            toOSD = type.GetMethod("Serialize");
            if (toOSD != null)
            {
                var osdv = toOSD.Invoke(value, new object[0]) as OSD;
                if (osdv != null)
                {
                    map.Add(s, osdv);
                    return;
                }
            }

            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
                if (type == typeof(int))
                {
                    map.Add(s, (int)value);
                    return;
                }
                if (type == typeof(uint))
                {
                    map.Add(s, (uint)value);
                    return;
                }
                if (type == typeof(byte))
                {
                    map.Add(s, (byte)value);
                    return;
                }
                if (type == typeof(sbyte))
                {
                    map.Add(s, (sbyte)value);
                    return;
                }

                if (type == typeof(long))
                {
                    map.Add(s, (long)value);
                    return;
                }
                if (type == typeof(ulong))
                {
                    map.Add(s, (ulong)value);
                    return;
                }
                if (type == typeof(short))
                {
                    map.Add(s, (short)value);
                    return;
                }
                if (type == typeof(ushort))
                {
                    map.Add(s, (ushort)value);
                    return;
                }

                try
                {
                    var i = (char)value;
                    map.Add(s, i);
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        var i = (uint)value;
                        map.Add(s, i);
                        return;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var i = (int)value;
                            map.Add(s, i);
                            return;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                var i = (ulong)value;
                                map.Add(s, i);
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            if (value is object[])
            {                
                var obj = (object[])value;
                OSD osda = FromArray(obj);
                map.Add(s, osda);
                return;
            }
            if (value is Array)
            {
                Array obj = (Array)value;
                OSDArray osda = new OSDArray(obj.Length);
                for (int i = 0; i < obj.Length; i++)
                {
                    object a = obj.GetValue(i);
                    osda[i] = GetOSD(a, type.GetElementType());
                }
                map.Add(s, osda);
                return;
            }
            var submap = new OSDMap();
            AddObjectOSD(value, submap, type);
            if (submap.Count > 1)
            {
                map.Add(s, submap);
                return;
            }
            return;
        }

        private static OSD GetOSD(object o, Type type)
        {
            if (o == null) return null;
            OSDMap map = new OSDMap();
            AddOSDMember(map, "value", o, type ?? o.GetType());
            return map["value"];
        }

        private static bool AddOSDMember0(OSDMap map, string s, object value, Type type)
        {
            if (value == null) return false;
            if (type == typeof(UUID))
            {
                var uuid = (UUID)value;
                if (!UUIDFactory.IsNullOrZero(uuid))
                {
                    map[s] = uuid;
                    return true;
                }
                map[s] = uuid;
                return true;
            }

            var searchTypes = new HashSet<Type>() { type };
            foreach (Type t in type.GetInterfaces())
            {
                searchTypes.Add(t);
            }
            Type st = type.BaseType;
            if (st != null) searchTypes.Add(st);
            Type valueGetType = value.GetType();
            if (type != valueGetType)
            {
                searchTypes.Add(valueGetType);
                foreach (Type t in valueGetType.GetInterfaces())
                {
                    searchTypes.Add(t);
                }
                st = valueGetType.BaseType;
                if (st != null) searchTypes.Add(st);
            }
            foreach (Type t in searchTypes)
            {
                bool found;
                var osd = ConvertOP(value, new Type[] { t }, typeof(OSD), typeof(OSD), out found);
                if (found)
                {
                    map.Add(s, (OSD)osd);
                    return true;
                }
            }
            return false;
        }
        public static object Convert0(object StartObject, Type EndType)
        {
            object retval = null;

            Type StartType = StartObject.GetType();
            retval = EndType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[0]);
            PropertyInfo[] pis = EndType.GetProperties(basePropertyFlags);
            foreach (PropertyInfo oI in pis)//you should probably use getFields, in case there is no setter, but in my case this is -safe-(ish)
            {
                PropertyInfo thisFIs = StartType.GetProperty(oI.Name, basePropertyFlags);
                if ((thisFIs != null))
                {
                    object cVal = StartType.InvokeMember(thisFIs.Name, basePropertyFlags | BindingFlags.GetProperty, null, StartObject, new object[0]);
                    if (thisFIs.PropertyType == oI.PropertyType)
                    { EndType.InvokeMember(thisFIs.Name, basePropertyFlags | BindingFlags.SetProperty, null, retval, new object[] { cVal }); }
                    else
                    {

                        //check for operator for it?
                        bool found;
                        object o = ConvertOP(cVal, null, oI.PropertyType, null, out found);
                        if (o != null)
                            EndType.InvokeMember(thisFIs.Name, basePropertyFlags | BindingFlags.SetProperty, null,
                                                 retval, new object[] { o });
                    }
                }
            }

            return retval;
        }

        public static string DefaultValue(OSDMap map, string helper, string empty)
        {
            return (map[helper]) ? map[helper].AsString() : empty;
        }

        public static OSD FromArray(object[] uuids)
        {
            if (uuids == null)
            {
                IsMissing();
                return new OSD();
            }
            if (uuids.Length == 0)
            {
                return new OSDArray();
            }
            object[] obj = (object[])uuids;
            OSDArray osda = new OSDArray(obj.Length);
            for (int i = 0; i < obj.Length; i++)
            {
                object a = obj[i];
                osda[i] = GetOSD(a, uuids.GetType().GetElementType());
            }
            return osda;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDBoolean : OSD
    {
        private bool value;

        private static byte[] trueBinary = { 0x31 };
        private static byte[] falseBinary = { 0x30 };

        public override OSDType Type { get { return OSDType.Boolean; } }

        public OSDBoolean(bool value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return value; }
        public override int AsInteger() { return value ? 1 : 0; }
        public override double AsReal() { return value ? 1d : 0d; }
        public override string AsString() { return value ? "1" : "0"; }
        public override byte[] AsBinary() { return value ? trueBinary : falseBinary; }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDInteger : OSD
    {
        private int value;

        public override OSDType Type { get { return OSDType.Integer; } }

        public OSDInteger(int value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return value != 0; }
        public override int AsInteger() { return value; }
        public override int AsIntegerD() { return value; }
        public override uint AsUInteger() { return (uint)value; }
        public override long AsLong() { return value; }
        public override ulong AsULong() { return (ulong)value; }
        public override double AsReal() { return (double)value; }
        public override string AsString() { return value.ToString(); }
        public override byte[] AsBinary() { return Utils.IntToBytesBig(value); }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDReal : OSD
    {
        private double value;

        public override OSDType Type { get { return OSDType.Real; } }

        public OSDReal(double value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return (!Double.IsNaN(value) && value != 0d); }
        
        public override int AsInteger()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)Int32.MaxValue)
                return Int32.MaxValue;
            if (value < (double)Int32.MinValue)
                return Int32.MinValue;
            return (int)Math.Round(value);
        }

        public override uint AsUInteger()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)UInt32.MaxValue)
                return UInt32.MaxValue;
            if (value < (double)UInt32.MinValue)
                return UInt32.MinValue;
            return (uint)Math.Round(value);
        }

        public override long AsLong()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)Int64.MaxValue)
                return Int64.MaxValue;
            if (value < (double)Int64.MinValue)
                return Int64.MinValue;
            return (long)Math.Round(value);
        }

        public override ulong AsULong()
        {
            if (Double.IsNaN(value))
                return 0;
            if (value > (double)UInt64.MaxValue)
                return Int32.MaxValue;
            if (value < (double)UInt64.MinValue)
                return UInt64.MinValue;
            return (ulong)Math.Round(value);
        }

        public override double AsReal() { return value; }
        // "r" ensures the value will correctly round-trip back through Double.TryParse
        public override string AsString() { return value.ToString("r", Utils.EnUsCulture); }
        public override byte[] AsBinary() { return Utils.DoubleToBytesBig(value); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDString : OSD
    {
        private string value;

        public override OSDType Type { get { return OSDType.String; } }

        public OSDString(string value)
        {
            // Refuse to hold null pointers
            if (value != null)
                this.value = string.Intern(value);
            else
                this.value = String.Empty;
        }

        public override bool AsBoolean()
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value == "0" || value.ToLower() == "false")
                return false;

            return true;
        }

        public override int AsInteger()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (int)Math.Floor(dbl);
            else
                return 0;
        }

        public override uint AsUInteger()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (uint)Math.Floor(dbl);
            else
                return 0;
        }

        public override long AsLong()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (long)Math.Floor(dbl);
            else
                return 0;
        }

        public override ulong AsULong()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (ulong)Math.Floor(dbl);
            else
                return 0;
        }

        public override double AsReal()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return dbl;
            else
                return 0d;
        }

        public override string  AsString() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(value); }
        public override UUID AsUUID()
        {
            UUID uuid = UUID.Zero;
            if (UUID.TryParse(value, out uuid))
                return uuid;
            else
                return UUID.Zero;
        }
        public override DateTime AsDate()
        {
            DateTime dt;
            if (DateTime.TryParse(value, out dt))
                return dt;
            else
                return Utils.Epoch;
        }
        public override Uri AsUri()
        {
            Uri uri;
            if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                return uri;
            else
                return null;
        }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDUUID : OSD
    {
        private UUID value = UUID.Zero;

        public override OSDType Type { get { return OSDType.UUID; } }

        public OSDUUID(UUID value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return (value == UUID.Zero) ? false : true; }
        public override string AsString() { return value.ToString(); }
        public override UUID AsUUID() { return value; }
        public override byte[] AsBinary() { return value.GetBytes(); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDDate : OSD
    {
        private DateTime value;

        public override OSDType Type { get { return OSDType.Date; } }

        public OSDDate(DateTime value)
        {
            this.value = value;
        }

        public override string AsString()
        {
            string format;
            if (value.Millisecond > 0)
                format = "yyyy-MM-ddTHH:mm:ss.ffZ";
            else
                format = "yyyy-MM-ddTHH:mm:ssZ";
            return value.ToUniversalTime().ToString(format);
        }

        public override int AsInteger()
        {
            return (int)Utils.DateTimeToUnixTime(value);
        }

        public override uint AsUInteger()
        {
            return Utils.DateTimeToUnixTime(value);
        }

        public override long AsLong()
        {
            return (long)Utils.DateTimeToUnixTime(value);
        }

        public override ulong AsULong()
        {
            return Utils.DateTimeToUnixTime(value);
        }

        public override byte[] AsBinary()
        {
            TimeSpan ts = value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Utils.DoubleToBytes(ts.TotalSeconds);
        }

        public override DateTime AsDate() { return value; }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDUri : OSD
    {
        private Uri value;

        public override OSDType Type { get { return OSDType.URI; } }

        public OSDUri(Uri value)
        {
            this.value = value;
        }

        public override string AsString()
        {
            if (value != null)
            {
                if (value.IsAbsoluteUri)
                    return value.AbsoluteUri;
                else
                    return value.ToString();
            }
            return string.Empty;
        }

        public override Uri AsUri() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(AsString()); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDBinary : OSD
    {
        private byte[] value;

        public override OSDType Type { get { return OSDType.Binary; } }

        public override bool AsBoolean()
        {
            return value != null && value.Length > 0;
        }

        public OSDBinary(byte[] value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = Utils.EmptyBytes;
        }

        public OSDBinary(uint value)
        {
            this.value = new byte[]
            {
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public OSDBinary(long value)
        {
            this.value = new byte[]
            {
                (byte)((value >> 56) % 256),
                (byte)((value >> 48) % 256),
                (byte)((value >> 40) % 256),
                (byte)((value >> 32) % 256),
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public OSDBinary(ulong value)
        {
            this.value = new byte[]
            {
                (byte)((value >> 56) % 256),
                (byte)((value >> 48) % 256),
                (byte)((value >> 40) % 256),
                (byte)((value >> 32) % 256),
                (byte)((value >> 24) % 256),
                (byte)((value >> 16) % 256),
                (byte)((value >> 8) % 256),
                (byte)(value % 256)
            };
        }

        public override string AsString() { return Convert.ToBase64String(value); }
        public override byte[] AsBinary() { return value; }

        public override uint AsUInteger()
        {
            return (uint)(
                (value[0] << 24) +
                (value[1] << 16) +
                (value[2] << 8) +
                (value[3] << 0));
        }

        public override long AsLong()
        {
            return (long)(
                ((long)value[0] << 56) +
                ((long)value[1] << 48) +
                ((long)value[2] << 40) +
                ((long)value[3] << 32) +
                ((long)value[4] << 24) +
                ((long)value[5] << 16) +
                ((long)value[6] << 8) +
                ((long)value[7] << 0));
        }

        public override ulong AsULong()
        {
            return (ulong)(
                ((ulong)value[0] << 56) +
                ((ulong)value[1] << 48) +
                ((ulong)value[2] << 40) +
                ((ulong)value[3] << 32) +
                ((ulong)value[4] << 24) +
                ((ulong)value[5] << 16) +
                ((ulong)value[6] << 8) +
                ((ulong)value[7] << 0));
        }

        public override string ToString()
        {
            return Utils.BytesToHexString(value, null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDMap : OSD, IDictionary<string, OSD>
    {
        private Dictionary<string, OSD> value;

        public override OSDType Type { get { return OSDType.Map; } }

        public OSDMap()
        {
            value = new Dictionary<string, OSD>();
        }

        public OSDMap(int capacity)
        {
            value = new Dictionary<string, OSD>(capacity);
        }

        public OSDMap(Dictionary<string, OSD> value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = new Dictionary<string, OSD>();
        }

        public override bool AsBoolean() { return value.Count > 0; }

        public override string ToString()
        {
            return OSDParser.SerializeJsonString(this, true);
        }

        #region IDictionary Implementation

        public int Count { get { return value.Count; } }
        public bool IsReadOnly { get { return false; } }
        public ICollection<string> Keys { get { return value.Keys; } }
        public ICollection<OSD> Values { get { return value.Values; } }
        public OSD this[string key]
        {
            get
            {
                OSD llsd;
                if (this.value.TryGetValue(key, out llsd))
                    return llsd;
                else
                    return new OSD();
            }
            set { this.value[string.Intern(key)] = value; }
        }

        public bool ContainsKey(string key)
        {
            return value.ContainsKey(key);
        }

        public void Add(string key, OSD llsd)
        {
            value.Add(string.Intern(key), llsd);
        }

        public void Add(KeyValuePair<string, OSD> kvp)
        {
            value.Add(string.Intern(kvp.Key), kvp.Value);
        }

        public bool Remove(string key)
        {
            return value.Remove(key);
        }

        public bool TryGetValue(string key, out OSD llsd)
        {
            return value.TryGetValue(key, out llsd);
        }

        public void Clear()
        {
            value.Clear();
        }

        public bool Contains(KeyValuePair<string, OSD> kvp)
        {
            // This is a bizarre function... we don't really implement it
            // properly, hopefully no one wants to use it
            return value.ContainsKey(kvp.Key);
        }

        public void CopyTo(KeyValuePair<string, OSD>[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, OSD> kvp)
        {
            return this.value.Remove(kvp.Key);
        }

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, OSD>> IEnumerable<KeyValuePair<string, OSD>>.GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        #endregion IDictionary Implementation
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class OSDArray : OSD, IList<OSD>
    {
        private List<OSD> value;

        public override OSDType Type { get { return OSDType.Array; } }

        public OSDArray()
        {
            value = new List<OSD>();
        }

        public OSDArray(int capacity)
        {
            value = new List<OSD>(capacity);
        }

        public OSDArray(List<OSD> value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = new List<OSD>();
        }

        public override byte[] AsBinary()
        {
            byte[] binary = new byte[value.Count];

            for (int i = 0; i < value.Count; i++)
                binary[i] = (byte)value[i].AsInteger();

            return binary;
        }

        public override long AsLong()
        {
            OSDBinary binary = new OSDBinary(AsBinary());
            return binary.AsLong();
        }

        public override ulong AsULong()
        {
            OSDBinary binary = new OSDBinary(AsBinary());
            return binary.AsULong();
        }

        public override uint AsUInteger()
        {
            OSDBinary binary = new OSDBinary(AsBinary());
            return binary.AsUInteger();
        }

        public override Vector2 AsVector2()
        {
            Vector2 vector = Vector2.Zero;

            if (this.Count == 2)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
            }

            return vector;
        }

        public override Vector3 AsVector3()
        {
            Vector3 vector = Vector3.Zero;

            if (this.Count == 3)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
                vector.Z = (float)this[2].AsReal();
            }

            return vector;
        }

        public override Vector3d AsVector3d()
        {
            Vector3d vector = Vector3d.Zero;

            if (this.Count == 3)
            {
                vector.X = this[0].AsReal();
                vector.Y = this[1].AsReal();
                vector.Z = this[2].AsReal();
            }

            return vector;
        }

        public override Vector4 AsVector4()
        {
            Vector4 vector = Vector4.Zero;

            if (this.Count == 4)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
                vector.Z = (float)this[2].AsReal();
                vector.W = (float)this[3].AsReal();
            }

            return vector;
        }

        public override Quaternion AsQuaternion()
        {
            Quaternion quaternion = Quaternion.Identity;

            if (this.Count == 4)
            {
                quaternion.X = (float)this[0].AsReal();
                quaternion.Y = (float)this[1].AsReal();
                quaternion.Z = (float)this[2].AsReal();
                quaternion.W = (float)this[3].AsReal();
            }

            return quaternion;
        }

        public override Color4 AsColor4()
        {
            Color4 color = Color4.Black;

            if (this.Count == 4)
            {
                color.R = (float)this[0].AsReal();
                color.G = (float)this[1].AsReal();
                color.B = (float)this[2].AsReal();
                color.A = (float)this[3].AsReal();
            }

            return color;
        }

        public override bool AsBoolean() { return value.Count > 0; }

        public override string ToString()
        {
            return OSDParser.SerializeJsonString(this, true);
        }

        #region IList Implementation

        public int Count { get { return value.Count; } }
        public bool IsReadOnly { get { return false; } }
        public OSD this[int index]
        {
            get { return value[index]; }
            set { this.value[index] = value; }
        }

        public int IndexOf(OSD llsd)
        {
            return value.IndexOf(llsd);
        }

        public void Insert(int index, OSD llsd)
        {
            value.Insert(index, llsd);
        }

        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
        }

        public void Add(OSD llsd)
        {
            value.Add(llsd);
        }

        public void Clear()
        {
            value.Clear();
        }

        public bool Contains(OSD llsd)
        {
            return value.Contains(llsd);
        }

        public bool Contains(string element)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Type == OSDType.String && value[i].AsString() == element)
                    return true;
            }

            return false;
        }

        public void CopyTo(OSD[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(OSD llsd)
        {
            return value.Remove(llsd);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator<OSD> IEnumerable<OSD>.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        #endregion IList Implementation
    }

    public partial class OSDParser
    {
        const string LLSD_BINARY_HEADER = "<? llsd/binary ?>";
        const string LLSD_XML_HEADER = "<llsd>";
        const string LLSD_XML_ALT_HEADER = "<?xml";
        const string LLSD_XML_ALT2_HEADER = "<? llsd/xml ?>";

        public static OSD Deserialize(byte[] data)
        {
            string header =string.Intern( Encoding.ASCII.GetString(data, 0, data.Length >= 17 ? 17 : data.Length));

            if (header.StartsWith(LLSD_BINARY_HEADER, StringComparison.InvariantCultureIgnoreCase))
            {
                return DeserializeLLSDBinary(data);
            }
            else if (header.StartsWith(LLSD_XML_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                header.StartsWith(LLSD_XML_ALT_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                header.StartsWith(LLSD_XML_ALT2_HEADER, StringComparison.InvariantCultureIgnoreCase))
            {
                return DeserializeLLSDXml(data);
            }
            else
            {
                return DeserializeJson(Encoding.UTF8.GetString(data));
            }
        }

        public static OSD Deserialize(string data)
        {
            if (data.StartsWith(LLSD_BINARY_HEADER, StringComparison.InvariantCultureIgnoreCase))
            {
                return DeserializeLLSDBinary(Encoding.UTF8.GetBytes(data));
            }
            else if (data.StartsWith(LLSD_XML_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                data.StartsWith(LLSD_XML_ALT_HEADER, StringComparison.InvariantCultureIgnoreCase) ||
                data.StartsWith(LLSD_XML_ALT2_HEADER, StringComparison.InvariantCultureIgnoreCase))
            {
                return DeserializeLLSDXml(data);
            }
            else
            {
                return DeserializeJson(data);
            }
        }

        public static OSD Deserialize(Stream stream)
        {
            if (stream.CanSeek)
            {
                byte[] headerData = new byte[14];
                stream.Read(headerData, 0, 14);
                stream.Seek(0, SeekOrigin.Begin);
                string header = Encoding.ASCII.GetString(headerData);

                if (header.StartsWith(LLSD_BINARY_HEADER))
                    return DeserializeLLSDBinary(stream);
                else if (header.StartsWith(LLSD_XML_HEADER) || header.StartsWith(LLSD_XML_ALT_HEADER) || header.StartsWith(LLSD_XML_ALT2_HEADER))
                    return DeserializeLLSDXml(stream);
                else
                    return DeserializeJson(stream);
            }
            else
            {
                throw new OSDException("Cannot deserialize structured data from unseekable streams");
            }
        }
    }
}
