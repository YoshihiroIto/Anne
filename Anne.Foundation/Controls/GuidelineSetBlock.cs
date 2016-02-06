using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    // 参考: http://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html

    public class GuidelineSetBlock : IDisposable
    {
        private readonly DrawingContext _target;

        public GuidelineSetBlock(
            DrawingContext target,
            IEnumerable<double> xGuides = null,
            IEnumerable<double> yGuides = null)
        {
            Debug.Assert(target != null);

            _target = target;

            var guidelines = new GuidelineSet();
            {
                xGuides?.ForEach(g => guidelines.GuidelinesX.Add(g));
                yGuides?.ForEach(g => guidelines.GuidelinesY.Add(g));
            }

            target.PushGuidelineSet(guidelines);
        }

        public GuidelineSetBlock(
            DrawingContext target,
            Rect rect,
            double rectPenWidth = 0,
            IEnumerable<double> xGuides = null,
            IEnumerable<double> yGuides = null)
        {
            Debug.Assert(target != null);

            _target = target;

            var guidelines = new GuidelineSet();
            {
                guidelines.GuidelinesX.Add(rect.Left + rectPenWidth*0.5);
                guidelines.GuidelinesX.Add(rect.Right + rectPenWidth*0.5);
                guidelines.GuidelinesY.Add(rect.Top + rectPenWidth*0.5);
                guidelines.GuidelinesY.Add(rect.Bottom + rectPenWidth*0.5);

                xGuides?.ForEach(g => guidelines.GuidelinesX.Add(g));
                yGuides?.ForEach(g => guidelines.GuidelinesY.Add(g));
            }

            target.PushGuidelineSet(guidelines);
        }

        public void Dispose()
        {
            _target.Pop();
        }
    }
}