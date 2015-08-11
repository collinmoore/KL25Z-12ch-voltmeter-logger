using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battery_charger_tester_gui
{
    class LabelManager
    {
        ArrayList uiLabels;

        public LabelManager(ArrayList labels)
        {
            this.uiLabels = labels;
        }

        public void addLabel(Label label)
        {
            this.uiLabels.Add(label);
        }

        public void removeLabel(int index)
        {
            this.uiLabels.RemoveAt(index);
        }

        public void setText(String text, int index)
        {
            ((Label)uiLabels[index]).Text = text;
        }

        public void setAllTo(String text)
        {
            foreach (Object i in uiLabels){
                ((Label)i).Text = text;
            }
        }


    }
}
