using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ch.kainoo.core
{
    using StringPtr = System.IntPtr;

    public delegate void EventCallback(StringPtr callLutId, StringPtr data);

    public static class CallLookupTable
    {
        private static Dictionary<string, Action<string>> _callbackRegistrations = new Dictionary<string, Action<string>>();



        [MonoPInvokeCallback(typeof(EventCallback))]
        public static void CallbackLut(StringPtr callLutId_, StringPtr data_)
        {
            string callLutId = Marshal.PtrToStringUTF8(callLutId_);
            string data = Marshal.PtrToStringUTF8(data_);
            InvokeCallback(callLutId, data, false);
        }

        [MonoPInvokeCallback(typeof(EventCallback))]
        public static void CallbackLutPreserve(StringPtr callLutId_, StringPtr data_)
        {
            string callLutId = Marshal.PtrToStringUTF8(callLutId_);
            string data = Marshal.PtrToStringUTF8(data_);
            InvokeCallback(callLutId, data, true);
        }



        private static string GenerateString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                var randomValue = UnityEngine.Random.Range(0, chars.Length - 1);
                stringChars[i] = chars[randomValue];
            }

            var finalString = new string(stringChars);
            return finalString;
        }

        public static string RegisterCallback(Action<string> callback)
        {
            var identifier = GenerateString(16);

            _callbackRegistrations.Add(identifier, callback);

            return identifier;
        }

        public static void InvokeCallback(string identifier, string data, bool preserve)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                Debug.LogError($"Argument null: {nameof(identifier)}");
                return;
            }

            if (!_callbackRegistrations.TryGetValue(identifier, out var callback))
            {
                Debug.LogError($"Identifier '{identifier}' not found in registered callbacks");
                return;
            }

            callback.Invoke(data);

            if (!preserve)
            {
                _callbackRegistrations.Remove(identifier);
            }

        }

    }
}