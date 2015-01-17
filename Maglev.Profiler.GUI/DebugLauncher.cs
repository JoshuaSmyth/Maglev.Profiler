using WorldEditor.Debugger;

namespace TinyFrog.Profiler.Gui
{
    public class DebugLauncher
    {
#if WINDOWS
        private MainDebuggerWindow m_DebuggerWindow;
#endif

        public DebugLauncher()
        {
#if WINDOWS
            m_DebuggerWindow = new MainDebuggerWindow();
#endif
        }

        public void LaunchDebugger()
        {
#if WINDOWS

            if (m_DebuggerWindow.IsLoaded == false)
            {
                m_DebuggerWindow = new MainDebuggerWindow();
            }

            m_DebuggerWindow.Show();

#endif
        }

        public void CloseDebugger()
        {
#if WINDOWS
            m_DebuggerWindow.Close();
#endif
        }
    }
}
