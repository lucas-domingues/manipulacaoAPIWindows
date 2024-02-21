using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    // Importa a função SendInput da user32.dll
    [DllImport("user32.dll")]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    // Estrutura de entrada para simular uma tecla pressionada
    struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    // Union para definir o formato da entrada
    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    // Estrutura para simular entrada de teclado
    struct KEYBDINPUT
    {
        public ushort wVk; // Código virtual da tecla
        public ushort wScan;
        public uint dwFlags; // Flags para indicar ações como pressionar, soltar ou simular um evento de pressionar e soltar
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    // Estrutura para simular entrada de mouse (não utilizada neste exemplo)
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    // Estrutura para simular entrada de hardware (não utilizada neste exemplo)
    struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    // Constantes para as ações do teclado
    const int INPUT_KEYBOARD = 1;
    const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    const int KEYEVENTF_KEYUP = 0x0002;
    const int VK_CAPITAL = 0x14;

    static void Main(string[] args)
    {
        while (true)
        {
            // Simula uma tecla sendo pressionada
            INPUT[] inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = VK_CAPITAL,
                            dwFlags = KEYEVENTF_EXTENDEDKEY
                        }
                    }
                }
            };

            // Envia a entrada simulada
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

            // Aguarda um pouco antes de repetir o processo
            Thread.Sleep(1000);
        }
    }
}
