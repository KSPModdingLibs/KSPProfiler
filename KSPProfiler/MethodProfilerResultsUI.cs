using KsmUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace KSPProfiler
{
    public class MethodProfilerResultsUI : KsmUIVerticalLayout
    {
        private KsmUIVerticalScrollView scrollView;

        public MethodProfilerResultsUI(KsmUIBase parent) : base(parent)
        {
            this.SetLayout(true, false, -1, 750);

            KsmUIHorizontalLayout uiOptions = new KsmUIHorizontalLayout(this)
                .SetLayout(true, false, -1, 18)
                .SetSpacing(10);

            new KsmUIButton(uiOptions)
                .SetLayout(100, 18)
                .SetText("Start")
                .SetButtonOnClick(MethodProfiler.InjectProfiler);

            new KsmUIButton(uiOptions)
                .SetLayout(100, 18)
                .SetText("Stop")
                .SetButtonOnClick(MethodProfiler.RemoveProfiler);

            scrollView = new KsmUIVerticalScrollView(this);
            scrollView.SetLayout(true, true);

            this.SetUpdateAction(Update, 2.5f);
        }

        private List<VirtualLine> virtualLines = new List<VirtualLine>();
        private int virtualLinesCount;

        private List<Line> lines = new List<Line>();

        private void Update()
        {
            int lineIndex = 0;
            foreach (ProfiledMethod method in ProfiledMethod.calledProfiledMethods.Values)
            {
                VirtualLine line;
                if (virtualLines.Count <= lineIndex)
                {
                    line = new VirtualLine();
                    virtualLines.Add(line);
                }
                else
                {
                    line = virtualLines[lineIndex];
                }

                line.SetMethod(method);
                lineIndex++;
            }

            virtualLinesCount = lineIndex;
            virtualLines.Sort((a, b) => b.meanTimePerFrame.CompareTo(a.meanTimePerFrame));

            for (int i = 0; i < virtualLinesCount; i++)
            {
                Line line;
                if (lines.Count <= i)
                {
                    line = new Line(this);
                    lines.Add(line);
                }
                else
                {
                    line = lines[i];
                }

                line.SetVirtualLine(virtualLines[i]);
            }
        }

        private class VirtualLine
        {
            public ProfiledMethod method;
            public double meanCallsPerFrame;
            public double meanTimePerFrame;

            public void SetMethod(ProfiledMethod method)
            {
                this.method = method;
                method.GetFrameStats(out meanCallsPerFrame, out meanTimePerFrame);
            }
        }

        private class Line : KsmUIBase
        {
            private VirtualLine virtualLine;

            private KsmUIText name;
            private KsmUIText meanPerFrame;
            private KsmUIText callCountPerFrame;
            private KsmUIText callCount;
            private KsmUIText mean;
            private KsmUIText median;
            private KsmUIText worst1;

            public Line(MethodProfilerResultsUI parent) : base(parent.scrollView)
            {
                this.SetLayout(true, false, -1, 15);

                name = new KsmUIText(this)
                    .SetStaticSizeAndPosition(200, 15);
                int hPos = 200;
                meanPerFrame = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                callCountPerFrame = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                callCount = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                mean = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                median = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                worst1 = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
            }

            public void Update(ProfiledMethod method)
            {
                name.Text = method.assemblyName + ":" + method.name;
                method.GetCallStats(out int callCountVal, out double meanVal, out double medianVal, out double percentVal);
                method.GetFrameStats(out double meanCallsPerFrame, out double meanTimePerFrame);
                meanPerFrame.Text = meanTimePerFrame.ToString("F5");
                callCountPerFrame.Text = meanCallsPerFrame.ToString("F1");
                callCount.Text = callCountVal.ToString();
                mean.Text = meanVal.ToString("F5");
                median.Text = medianVal.ToString("F5");
                worst1.Text = percentVal.ToString("F5");
            }

            internal void SetVirtualLine(VirtualLine virtualLine)
            {
                this.virtualLine = virtualLine;
                name.Text = virtualLine.method.name;
                meanPerFrame.Text = virtualLine.meanTimePerFrame.ToString("F5");
            }
        }

    }
}
