#if !NO_UI_WEB_BROWSER
namespace Internal.Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Internal.Xilium.CefGlue.Interop;

    /// <summary>
    /// Implement this interface to handle events when window rendering is disabled.
    /// The methods of this class will be called on the UI thread.
    /// </summary>
    public abstract unsafe partial class CefRenderHandler
    {
        private int get_root_screen_rect(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_rect = new CefRectangle();

            var result = GetRootScreenRect(m_browser, ref m_rect);

            if (result)
            {
                rect->x = m_rect.X;
                rect->y = m_rect.Y;
                rect->width = m_rect.Width;
                rect->height = m_rect.Height;
                return 1;
            }
            else return 0;
        }

        /// <summary>
        /// Called to retrieve the root window rectangle in screen coordinates. Return
        /// true if the rectangle was provided.
        /// </summary>
        protected virtual bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            // TODO: return CefRectangle? (Nullable<CefRectangle>) instead of returning bool?
            return false;
        }


        private int get_view_rect(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_rect = new CefRectangle();

            var result = GetViewRect(m_browser, ref m_rect);

            if (result)
            {
                rect->x = m_rect.X;
                rect->y = m_rect.Y;
                rect->width = m_rect.Width;
                rect->height = m_rect.Height;
                return 1;
            }
            else return 0;
        }

        /// <summary>
        /// Called to retrieve the view rectangle which is relative to screen
        /// coordinates. Return true if the rectangle was provided.
        /// </summary>
        protected virtual bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            // TODO: return CefRectangle? (Nullable<CefRectangle>) instead of returning bool?
            return false;
        }


        private int get_screen_point(cef_render_handler_t* self, cef_browser_t* browser, int viewX, int viewY, int* screenX, int* screenY)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);

            int m_screenX = 0;
            int m_screenY = 0;

            var result = GetScreenPoint(m_browser, viewX, viewY, ref m_screenX, ref m_screenY);

            if (result)
            {
                *screenX = m_screenX;
                *screenY = m_screenY;
                return 1;
            }
            else return 0;
        }

        /// <summary>
        /// Called to retrieve the translation from view coordinates to actual screen
        /// coordinates. Return true if the screen coordinates were provided.
        /// </summary>
        protected virtual bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            return false;
        }


        private int get_screen_info(cef_render_handler_t* self, cef_browser_t* browser, cef_screen_info_t* screen_info)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_screenInfo = new CefScreenInfo(screen_info);

            var result = GetScreenInfo(m_browser, m_screenInfo);

            m_screenInfo.Dispose();
            m_browser.Dispose();

            return result ? 1 : 0;
        }

        /// <summary>
        /// Called to allow the client to fill in the CefScreenInfo object with
        /// appropriate values. Return true if the |screen_info| structure has been
        /// modified.
        /// If the screen info rectangle is left empty the rectangle from GetViewRect
        /// will be used. If the rectangle is still empty or invalid popups may not be
        /// drawn correctly.
        /// </summary>
        protected abstract bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo);


        private void on_popup_show(cef_render_handler_t* self, cef_browser_t* browser, int show)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);

            OnPopupShow(m_browser, show != 0);
        }

        /// <summary>
        /// Called when the browser wants to show or hide the popup widget. The popup
        /// should be shown if |show| is true and hidden if |show| is false.
        /// </summary>
        protected virtual void OnPopupShow(CefBrowser browser, bool show)
        {
        }


        private void on_popup_size(cef_render_handler_t* self, cef_browser_t* browser, cef_rect_t* rect)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_rect = new CefRectangle(rect->x, rect->y, rect->width, rect->height);

            OnPopupSize(m_browser, m_rect);
        }

        /// <summary>
        /// Called when the browser wants to move or resize the popup widget. |rect|
        /// contains the new location and size in view coordinates.
        /// </summary>
        protected abstract void OnPopupSize(CefBrowser browser, CefRectangle rect);


        private void on_paint(cef_render_handler_t* self, cef_browser_t* browser, CefPaintElementType type, UIntPtr dirtyRectsCount, cef_rect_t* dirtyRects, void* buffer, int width, int height)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);

            // TODO: reuse arrays?
            var m_dirtyRects = new CefRectangle[(int)dirtyRectsCount];

            var count = (int)dirtyRectsCount;
            var rect = dirtyRects;
            for (var i = 0; i < count; i++)
            {
                m_dirtyRects[i].X = rect->x;
                m_dirtyRects[i].Y = rect->y;
                m_dirtyRects[i].Width = rect->width;
                m_dirtyRects[i].Height = rect->height;

                rect++;
            }

            OnPaint(m_browser, type, m_dirtyRects, (IntPtr)buffer, width, height);
        }

        /// <summary>
        /// Called when an element should be painted. Pixel values passed to this
        /// method are scaled relative to view coordinates based on the value of
        /// CefScreenInfo.device_scale_factor returned from GetScreenInfo. |type|
        /// indicates whether the element is the view or the popup widget. |buffer|
        /// contains the pixel data for the whole image. |dirtyRects| contains the set
        /// of rectangles in pixel coordinates that need to be repainted. |buffer| will
        /// be |width|*|height|*4 bytes in size and represents a BGRA image with an
        /// upper-left origin.
        /// </summary>
        protected abstract void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height);


        private void on_cursor_change(cef_render_handler_t* self, cef_browser_t* browser, IntPtr cursor, CefCursorType type, cef_cursor_info_t* custom_cursor_info)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_cefCursorInfo = type == CefCursorType.Custom ? new CefCursorInfo(custom_cursor_info) : null;

            OnCursorChange(m_browser, cursor, type, m_cefCursorInfo);

            if (m_cefCursorInfo != null) m_cefCursorInfo.Dispose();
        }

        /// <summary>
        /// Called when the browser's cursor has changed. If |type| is CT_CUSTOM then
        /// |custom_cursor_info| will be populated with the custom cursor information.
        /// </summary>
        protected abstract void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo);


        private int start_dragging(cef_render_handler_t* self, cef_browser_t* browser, cef_drag_data_t* drag_data, CefDragOperationsMask allowed_ops, int x, int y)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);
            var m_dragData = CefDragData.FromNative(drag_data);

            var m_result = StartDragging(m_browser, m_dragData, allowed_ops, x, y);

            return m_result ? 1 : 0;
        }

        /// <summary>
        /// Called when the user starts dragging content in the web view. Contextual
        /// information about the dragged content is supplied by |drag_data|.
        /// (|x|, |y|) is the drag start location in screen coordinates.
        /// OS APIs that run a system message loop may be used within the
        /// StartDragging call.
        /// Return false to abort the drag operation. Don't call any of
        /// CefBrowserHost::DragSource*Ended* methods after returning false.
        /// Return true to handle the drag operation. Call
        /// CefBrowserHost::DragSourceEndedAt and DragSourceSystemDragEnded either
        /// synchronously or asynchronously to inform the web view that the drag
        /// operation has ended.
        /// </summary>
        protected virtual bool StartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y)
        {
            return false;
        }


        private void update_drag_cursor(cef_render_handler_t* self, cef_browser_t* browser, CefDragOperationsMask operation)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);

            UpdateDragCursor(m_browser, operation);
        }

        /// <summary>
        /// Called when the web view wants to update the mouse cursor during a
        /// drag and drop operation. |operation| describes the allowed operation
        /// (none, move, copy, link).
        /// </summary>
        protected virtual void UpdateDragCursor(CefBrowser browser, CefDragOperationsMask operation)
        {
        }


        private void on_scroll_offset_changed(cef_render_handler_t* self, cef_browser_t* browser, double x, double y)
        {
            CheckSelf(self);

            var m_browser = CefBrowser.FromNative(browser);

            OnScrollOffsetChanged(m_browser, x, y);
        }

        /// <summary>
        /// Called when the scroll offset has changed.
        /// </summary>
        protected abstract void OnScrollOffsetChanged(CefBrowser browser, double x, double y);
    }
}

#endif