using System;
using System.Runtime.InteropServices;

namespace Sinusoida
{
    public static class SinusoidApplication
    {
        // Stałe dla komunikatów API Windows
        private const int WM_CLOSE = 0x0010; // Zamknięcie okna
        private const int WM_PAINT = 0x000F; // Odświeżenie okna

        private const int IDC_ARROW = 32512; // Identyfikator kursora strzałki
        private const int COLOR_WINDOW = 5; // Kolor tła okna

        private const uint WS_OVERLAPPED = 0x00000000; // Styl okna: nakładające się
        private const uint WS_CAPTION = 0x00C00000; // Styl okna: z tytułem
        private const uint WS_SYSMENU = 0x00080000; // Styl okna: z menu systemowym

        private const uint CS_HREDRAW = 0x0002; // Przerysuj poziomo przy zmianie rozmiaru
        private const uint CS_VREDRAW = 0x0001; // Przerysuj pionowo przy zmianie rozmiaru

        // Deklaracje funkcji API Windows
        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        private static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("gdi32.dll")]
        private static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        private static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        private static void Main()
        {
            var hInstance = GetModuleHandle(null);

            var wcex = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = WndProc,
                hInstance = hInstance,
                hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                hbrBackground = (IntPtr)(1 + COLOR_WINDOW),
                lpszClassName = "SinusoidWindowClass"
            };

            RegisterClassEx(ref wcex);

            var hwnd = CreateWindowEx(
                0,
                "SinusoidWindowClass",
                "Sinusoid Application",
                WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU,
                100,
                100,
                800,
                600,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero
            );

            ShowWindow(hwnd, 1);

            while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private static IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case WM_PAINT:
                    var hdc = BeginPaint(hWnd, out var ps);
                    DrawSinusoid(hdc, ps.rcPaint);
                    EndPaint(hWnd, ref ps);
                    break;
                case WM_CLOSE:
                    PostQuitMessage(0);
                    break;
                default:
                    return DefWindowProc(hWnd, message, wParam, lParam);
            }

            return IntPtr.Zero;
        }

        private static void DrawSinusoid(IntPtr hdc, RECT rcPaint)
        {
            var width = rcPaint.Right - rcPaint.Left;
            var height = rcPaint.Bottom - rcPaint.Top;
            var midY = height / 2;

            var x = rcPaint.Left;
            var y = (int)(Math.Sin((double)x / width * 2 * Math.PI) * midY + midY);
            MoveToEx(hdc, x, y, IntPtr.Zero);

            for (x = rcPaint.Left + 1; x < rcPaint.Right; x++)
            {
                y = (int)(Math.Sin((double)x / width * 2 * Math.PI) * midY + midY);
                LineTo(hdc, x, y);
            }
        }

        // Struktury API Windows
        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WNDPROC lpfnWndProc;

            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        // Delegat funkcji obsługi komunikatów okna
        private delegate IntPtr WNDPROC(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
    }
}