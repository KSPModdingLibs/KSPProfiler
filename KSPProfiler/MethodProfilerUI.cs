using KsmUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSPProfiler
{
    public class MethodProfilerUI : KsmUIVerticalScrollView
    {
        public MethodProfilerUI(KsmUIBase parent) : base(parent)
        {
            this.SetLayout(true, false, -1, 750);

            MethodProfiler.Init();

            foreach (ProfiledMethodAssemblyGroup group in MethodProfiler.allAssemblyGroups)
            {
                KsmUIVerticalLayout groupLayout = new KsmUIVerticalLayout(this);

                KsmUIHeader header = new KsmUIHeader(groupLayout, group.assemblyName + " " + group.assemblyVersion + "\n" + group.assemblyLocation);

                foreach (ProfiledMethod method in group.methods)
                {
                    new KsmUIText(groupLayout, method.name).SetLayout(-1, -1);
                }
            }

        }

    }
}
