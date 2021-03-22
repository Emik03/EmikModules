using System.Collections.Generic;

namespace QuaverModule
{
    internal class Init
    {
        internal Init(QuaverScript quaver)
        {
            correctValues = new List<int[]>();
            this.quaver = quaver;

            generate = new Generate(quaver);
            select = new Select(quaver);
            render = quaver.Render;
        }

        internal Generate generate;
        internal Select select;
        internal QuaverScript quaver;
        internal RenderScript render;

        internal static bool anotherQuaverReady;
        internal bool solved, gameplay, ready, canAdjustScroll;
        internal static int moduleIdCounter;
        internal int moduleId;
        internal List<int[]> correctValues;

        internal void OnActivate()
        {
            // Resets static values if the bomb was quit out while the module was in submission.
            anotherQuaverReady = false;
            ArrowScript.arrowsPerColumn = new int[4];

            render.UpdateSelection();
            moduleId = ++moduleIdCounter;

            for (int i = 0; i < quaver.Buttons.Length; i++)
            {
                int j = i;
                quaver.Buttons[j].OnInteract += select.Press(j);
            }
        }
    }
}
