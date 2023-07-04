using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Component = UnityEngine.Component;

namespace LoU
{
    internal static class Extensions
    {
        
        internal static bool IsNull<T>(this T source)
        {
            return source == null;
        }

        internal static bool IsNotNull<T>(this T source)
        {
            return !source.IsNull();
        }
        
        internal static bool IsEmpty<T>(this T source)
        {
            if (typeof(T) == typeof(string))
                return (string) (object) source ==  "";
            return ((ICollection<object>) source).Count == 0;
        }

        internal static bool IsNotEmpty<T>(this T source)
        {
            return !source.IsEmpty();
        }
        internal static bool IsNullOrEmpty<T>(this T source)
        {
            return source.IsNull() || source.IsEmpty();
        }
        
        internal static bool IsNotNullOrEmpty<T>(this T source)
        {
            return !source.IsNullOrEmpty();
        }
        
        internal static bool Contains2(this string source, string toCheck)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck)) return false;
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool IsDictionary(this object o)
        {
            if (o.IsNull()) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
        
        internal static float DistanceFrom(this Vector3 a, Vector3 b)
        {
            if (a.IsNull() || b.IsNull()) return 0;
            var num1 = a.x - b.x;
            var num2 = a.y - b.y;
            var num3 = a.z - b.z;
            return (float) Math.Sqrt((double) num1 * num1 + (double) num2 * num2 + (double) num3 * num3);
        }
        
        internal static Vector3 RelativePositionFrom(this Transform transform, Transform ancestor)
        {
            var position = transform.localPosition;
            var t = transform.parent;
            
            while (t.IsNotNull() && t != ancestor)
            {
                position = position + t.localPosition;
                t = t.parent;
            }
            return position;
        }

        internal static void Enumerate<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }
    }

    internal static class Utils
    {

        public static Dictionary<string, FloatingPanel> FindPanelByName(string name)
        {

            var foundPanels = new Dictionary<string, FloatingPanel>();
            var fpm = FloatingPanelManager.DJCGIMIDOPB;
            if (!fpm) return foundPanels;

            var panels = (List<FloatingPanel>) InstanceFields.GetInstanceField(fpm, "AGLMPFPPEDK");
            if (panels.IsNullOrEmpty()) return foundPanels;

            foreach (var floatingPanel in panels)
            {
                if (name.IsNullOrEmpty() || floatingPanel.PanelId.Contains2(name))
                    foundPanels.Add(floatingPanel.PanelId, floatingPanel);
            }

            Logging.Log($"[FindPanelByName] - Found {foundPanels.Count} panels by {name}.");
            return foundPanels;
        }

        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(string name)
        {

            var objects = Enumerable.ToList(ClientObjectManager.DJCGIMIDOPB.MFGFGOCNCDG);
            Dictionary<string, DynamicObject> foundObjects = new Dictionary<string, DynamicObject>();

            foreach (var obj in objects)
            {
                if (obj.EBHEDGHBHGI.Contains2(name))
                    foundObjects.Add(obj.ObjectId.ToString(), obj);
            }

            Logging.Log($"[FindDynamicObjectsByName] - Found {foundObjects.Count()} dynamic objects by {name}.");
            return foundObjects;
        }

        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(string name, ulong containerId)
        {
            var foundObjects = new Dictionary<string, DynamicObject>();
            var objects = ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId);

            foreach (DynamicObject obj in objects)
            {
                if (name.IsNullOrEmpty() || obj.EBHEDGHBHGI.Contains2(name))
                    foundObjects.Add(obj.ObjectId.ToString(), obj);
            }

            Logging.Log(
                $"[FindDynamicObjectsByName] - Found {foundObjects.Count} dynamic objects by {name} and {containerId}.");
            return foundObjects;
        }
        
        public static DynamicObject FindDynamicObject(ulong objectId, ulong containerId = 0)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                        return obj;
                }
            }
            else
            {
                return ClientObjectManager.DJCGIMIDOPB.GetDynamicObjectById(objectId);
            }

            Logging.Log($"[FindDynamicObject] - Did not find Dynamic Object by id {objectId} and {containerId}.");
            return null;
        }

        public static ClientObject FindClientObject(ulong objectId, ulong containerId = 0)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                        return obj.GetComponent<ClientObject>();
                }
            }
            
            return ClientObjectManager.DJCGIMIDOPB.GetClientObjectById(objectId);
        }

        public static ClientObject FindPermanentObject(int permanentId)
        {
            return ClientObjectManager.DJCGIMIDOPB.GetPermanentObjectById(permanentId);
        }

        public static Dictionary<string, ClientObject> FindPermanentObjectByName(string name, float distance = 50)
        {
            var foundObjects = new Dictionary<string, ClientObject>();

            IEnumerable objects = ClientObjectManager.DJCGIMIDOPB.PermanentObjectLookup.Values.OrderBy(obj =>
                Vector3.Distance(obj.transform.position,
                    GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position));

            foreach (ClientObject obj in objects)
            {
                if (Vector3.Distance(obj.transform.position,
                    GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position) > distance)
                    break;

                if (obj.name.IsNotNullOrEmpty() && (name.IsEmpty() || obj.name.Contains2(name)))
                    foundObjects.Add(obj.PermanentId.ToString(), obj);
            }

            Logging.Log(
                $"[FindPermanentObjectByName] - Found {foundObjects.Count} objects by {name} with dist of {distance}.");
            return foundObjects;
        }

        public static IEnumerable<MobileInstance> GetNearbyMobiles(float distance)
        {
            if (ClientObjectManager.DJCGIMIDOPB.IsNotNull())
                return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(distance);
            return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(50);
        }

        public static MobileInstance GetMobile(ulong objectId)
        {
            return ClientObjectManager.DJCGIMIDOPB.GetMobileObjectById(objectId);
        }

        public static List<MobileInstance> FindMobile(string name, float distance = 50)
        {
            var playerPosition = GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position;
            var foundMobiles = new List<MobileInstance>();
            var nearbyMobiles = GetNearbyMobiles(distance).ToArray();
            if (nearbyMobiles.IsNullOrEmpty()) return foundMobiles;

            var mobiles = nearbyMobiles.OrderBy(obj =>
                obj.transform.position.DistanceFrom(playerPosition)).ToArray();

            if (mobiles.IsNullOrEmpty()) return foundMobiles;

            foreach (MobileInstance mobile in mobiles)
            {
                if (mobile.transform.position.DistanceFrom(playerPosition) > distance)
                {
                    break;
                }

                if (mobile.EBHEDGHBHGI.IsNotNullOrEmpty() &&
                    (name.IsEmpty() || mobile.EBHEDGHBHGI.Contains2(name) && !foundMobiles.Contains(mobile)))
                {
                    foundMobiles.Add(mobile);
                }
            }

            Logging.Log(
                $"[FindMobile] - Found {foundMobiles.Count} objects by {name} with dist of {distance}.");
            return foundMobiles;
        }

    }

    internal static class InstanceFields
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            if (instance.IsNull()) return null;
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        public static object GetInstanceField<T>(T instance, string fieldName)
        {
            if (instance.IsNull()) return null;
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = typeof(T).GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        public static void SetInstanceField<T>(T instance, string fieldName, object value)
        {
            if (instance.IsNull() || value.IsNull()) return;
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = typeof(T).GetField(fieldName, bindFlags);
            field?.SetValue(instance, value);
        }
    }

    internal static class Logging {
        
        private static bool DEBUGGING = false;

        public static void Log(string s)
        {
            if (DEBUGGING) Debug.Log($"LoU - {DateTime.UtcNow:o} - {s}");
        }

        private static void Log(GameObject c)
        {
            if (!DEBUGGING) return;
            
            Log( $"--- GAME OBJECT {c.GetInstanceID()} START ---");
            
            if (c.gameObject && c.gameObject.GetComponent<UIEventListener>())
                Log($"GameObject Name: {c.gameObject.name} Tag: {c.gameObject.tag} Has Listener!");
            if (c.transform && c.transform.GetComponent<UIEventListener>())
                Log($"Transform Name: {c.transform.name} Tag: {c.transform.tag} Has Listener!");
            
            Log($"Enumerating Through Components {c.GetComponents<Component>()}");

            c.GetComponents<Component>().Enumerate((comp, i) =>
            {
                Log($"i: {i.ToString()} | name: {comp.name} | tag: {comp.tag} | type: {comp.GetType()}");

                if (comp.GetComponent<UIEventListener>()) return;
                    Log($"{comp.name} has listener!");
                if (comp.transform.IsNotNull())
                    Log($"{comp.name}.transform has listener!");
                if (comp.gameObject.IsNotNull())
                    Log($"{comp.name}.gameObject has listener!");
                
                LogProps(comp);
            });
            Log( $"--- GAME OBJECT {c.GetInstanceID()} End ---");
        }

        public static void LogObject(DynamicObject obj)
        {
            if (!DEBUGGING) return;
            
            Log( $"--- OBJECT {obj.GetInstanceID()} START ---");
            LogProps(obj);
            Log("*** CLIENT OBJECT ***");
            LogProps(obj.AOJMJNFMBJO);
            Log("*** TRANSFORM ***");
            LogProps(obj.transform);
            Log("*** GAME OBJECT ***");
            LogProps(obj.gameObject);
            Log( $"--- OBJECT {obj.GetInstanceID()} END ---");
        }

        private static void LogProps(object obj)
        {
            if (!DEBUGGING) return;
            
            Log("--- PROPS START ---");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var name = descriptor.Name;
                var value = descriptor.GetValue(obj);
                Log($"{name} = {value}");
                
                if (!value.IsDictionary()) continue;
                var collection = ((IDictionary) value)?.Keys;
                
                if (collection == null) continue;
                foreach (string key in collection)
                {
                    Log($"Key:{name}.{key} = Value:{name}.{((IDictionary) value)[key]}");
                }
            }

            foreach (var f in obj.GetType().GetFields().Where(f => f.IsPrivate | f.IsPublic | f.IsStatic))
            {
                Log($"Field: {f.Name} Value: {f.GetValue(obj)}");
            }
            Log("--- PROPS END ---");
        }
    }
}
