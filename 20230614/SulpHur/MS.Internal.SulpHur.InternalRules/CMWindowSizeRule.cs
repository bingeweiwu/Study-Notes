﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.SulpHur.UICompliance;

namespace MS.Internal.SulpHur.CMRules
{
    public class WindowSizeRule : UIComplianceRuleBase
    {
        public override string Name
        {
            get { return @"Window Size Rule"; }
        }

        public override string Description
        {
            get { return @"Ensures window does not exceed a specific size"; }
        }

        public WindowSizeRule()
        {
            ruleProperties = new WindowSizeRuleProperties();
        }

        public override List<UIComplianceResultBase> UIVerify(ICollection<ElementInformation> controls)
        {
            List<UIComplianceResultBase> results = new List<UIComplianceResultBase>();

            IEnumerator<ElementInformation> enumerator = controls.GetEnumerator();
            enumerator.Reset();
            enumerator.MoveNext();
            ElementInformation rootControl = enumerator.Current;
            while (rootControl.Parent != null)
                rootControl = rootControl.Parent;

            if (rootControl.BoundingRectangle.Width > ruleProperties.MaxWidth ||
                rootControl.BoundingRectangle.Height > ruleProperties.MaxHeight)
            {
                UIComplianceResultBase result = new UIComplianceResultBase(ResultType.Warning, "The window size (" + rootControl.BoundingRectangle.Width + "," + rootControl.BoundingRectangle.Height + ") exceeds the maximum size (" + ruleProperties.MaxWidth + "," + ruleProperties.MaxHeight + ")", Name);
                //result.TakePicture = true;
                results.Add(result);
            }
            else
            {
                results.Add(new UIComplianceResultBase(ResultType.Pass, "The window size (" + rootControl.BoundingRectangle.Width + "," + rootControl.BoundingRectangle.Height + ") does not exceed the maximum size (" + ruleProperties.MaxWidth + "," + ruleProperties.MaxHeight + ")", Name));
            }

            return results;
        }

        private WindowSizeRuleProperties ruleProperties;

        public override List<UIComplianceResultBase> UIVerify(ICollection<WebElementInfo> webElements)
        {
            throw new NotImplementedException();
        }
    }

    public class WindowSizeRuleProperties
    {
        public int MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }
        private int maxWidth = 800;

        public int MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; }
        }
        private int maxHeight = 600;
    }
}
