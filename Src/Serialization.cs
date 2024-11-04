using CdReader.Sony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CdReader {
    public class Serialization {

        public interface IStreamMarshaler {
            object Read(Stream s);
        }


        [System.AttributeUsage(System.AttributeTargets.Struct)]
        public class StreamMarshalerAttribute : System.Attribute {
            public Type MarshalerType;

            public StreamMarshalerAttribute(Type marshalerType) {
                MarshalerType = marshalerType;
            }
        }


        public class StreamReader {
            private static byte[] _byteBuffer = new byte[1];

            public static byte ReadByte(Stream s) {
                s.Read(_byteBuffer, 0, 1);
                return _byteBuffer[0];
            }

            public static T ReadStruct<T>(Stream s) where T : struct {
                byte[] buffer = new byte[Marshal.SizeOf<T>()];
                s.Read(buffer, 0, buffer.Length);
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                handle.Free();
                return result;
            }

            public static string ReadStringAnsi(Stream s, long stringLength) {
                byte[] buffer = new byte[stringLength];
                s.Read(buffer, 0, buffer.Length);
                return System.Text.Encoding.ASCII.GetString(buffer);
            }


            public static T Read<T>(Stream s) where T : struct {
                var t = typeof(T);

                var resultSerializerAttribute = t.GetCustomAttribute<StreamMarshalerAttribute>();
                if(resultSerializerAttribute != null) {
                    var marshaler = (IStreamMarshaler)Activator.CreateInstance(resultSerializerAttribute.MarshalerType);
                    return (T)marshaler.Read(s);
                }

                
                object result = new T();


                var fields = t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
                foreach (var field in fields) {
                    var ft = field.FieldType;

                    var serializerAttribute = ft.GetCustomAttribute<StreamMarshalerAttribute>();


                    if (serializerAttribute == null) {
                        if (ft.IsArray) {
                            var marshalAttr = field.GetCustomAttribute<MarshalAsAttribute>();
                            if (marshalAttr == null) {
                                throw new Exception();
                            }


                            var elementType = ft.GetElementType();
                            var arr = Array.CreateInstance(elementType, marshalAttr.SizeConst);
                            var handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
                            var unmanagedSize = Marshal.SizeOf(elementType) * marshalAttr.SizeConst;
                            byte[] fieldBuffer = new byte[unmanagedSize];
                            s.Read(fieldBuffer, 0, fieldBuffer.Length);
                            Marshal.Copy(fieldBuffer, 0, handle.AddrOfPinnedObject(), fieldBuffer.Length);
                            handle.Free();
                            field.SetValue(result, arr);

                        } else if (ft == typeof(string)) {
                            var marshalAttr = field.GetCustomAttribute<MarshalAsAttribute>();
                            if (marshalAttr == null) {
                                throw new Exception();
                            }
                            byte[] fieldBuffer = new byte[marshalAttr.SizeConst];
                            s.Read(fieldBuffer, 0, fieldBuffer.Length);

                            var handle = GCHandle.Alloc(fieldBuffer, GCHandleType.Pinned);
                            var str = Marshal.PtrToStringAnsi(handle.AddrOfPinnedObject());
                            handle.Free();
                            field.SetValue(result, str);
                        } else {
                            var unmanagedSize = Marshal.SizeOf(ft);
                            byte[] fieldBuffer = new byte[unmanagedSize];
                            s.Read(fieldBuffer, 0, fieldBuffer.Length);
                            var handle = GCHandle.Alloc(fieldBuffer, GCHandleType.Pinned);
                            var fv = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), ft);
                            handle.Free();
                            field.SetValue(result, fv);
                        }
                    } else {
                        var marshaler = (IStreamMarshaler)Activator.CreateInstance(serializerAttribute.MarshalerType);
                        var fv = marshaler.Read(s);
                        field.SetValue(result, fv);
                    }

                }
                return (T)result;
            }
        }
    }
}
