#if !NO_UI_WEB_BROWSER
//
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
//
namespace Internal.Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Internal.Xilium.CefGlue.Interop;
    
    // Role: PROXY
    public sealed unsafe partial class CefV8StackFrame : IDisposable
    {
        internal static CefV8StackFrame FromNative(cef_v8stack_frame_t* ptr)
        {
            return new CefV8StackFrame(ptr);
        }
        
        internal static CefV8StackFrame FromNativeOrNull(cef_v8stack_frame_t* ptr)
        {
            if (ptr == null) return null;
            return new CefV8StackFrame(ptr);
        }
        
        private cef_v8stack_frame_t* _self;
        
        private CefV8StackFrame(cef_v8stack_frame_t* ptr)
        {
            if (ptr == null) throw new ArgumentNullException("ptr");
            _self = ptr;
        }
        
        ~CefV8StackFrame()
        {
            if (_self != null)
            {
                Release();
                _self = null;
            }
        }
        
        public void Dispose()
        {
            if (_self != null)
            {
                Release();
                _self = null;
            }
            GC.SuppressFinalize(this);
        }
        
        internal void AddRef()
        {
            cef_v8stack_frame_t.add_ref(_self);
        }
        
        internal bool Release()
        {
            return cef_v8stack_frame_t.release(_self) != 0;
        }
        
        internal bool HasOneRef
        {
            get { return cef_v8stack_frame_t.has_one_ref(_self) != 0; }
        }
        
        internal cef_v8stack_frame_t* ToNative()
        {
            AddRef();
            return _self;
        }
    }
}

#endif