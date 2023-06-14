using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.UICompliance
{
    public enum ControlType
    {
       // Summary:
        //     Identifies a button control.
         Button,
        //
        // Summary:
        //     Identifies a calendar control, such as a date-picker.
         Calendar,
        //
        // Summary:
        //     Identifies a check box control.
         CheckBox,
        //
        // Summary:
        //     Identifies a combo box control.
         ComboBox,
        //
        // Summary:
        //     Identifies a control that is not one of the defined control types.
         Custom,
        //
        // Summary:
        //     Identifies a data grid control.
         DataGrid,
        //
        // Summary:
        //     Identifies a data item control.
         DataItem,
        //
        // Summary:
        //     Identifies a document control.
         Document,
        //
        // Summary:
        //     Identifies an edit control, such as a text box.
         Edit,
        //
        // Summary:
        //     Identifies a group control, which acts as a container for other controls.
         Group,
        //
        // Summary:
        //     Identifies a header control, which is a container for the labels of rows
        //     and columns of information.
         Header,
        //
        // Summary:
        //     Identifies a header item, which is the label for a row or column of information.
         HeaderItem,
        //
        // Summary:
        //     Identifies a hyperlink control.
         Hyperlink,
        //
        // Summary:
        //     Identifies an image control.
         Image,
        //
        // Summary:
        //     Identifies a list control, such as a list box.
         List,
        //
        // Summary:
        //     Identifies a list item control, which is a child item of a list control.
         ListItem,
        //
        // Summary:
        //     Identifies a menu control, such as a top-level menu in an application window.
         Menu,
        //
        // Summary:
        //     Identifies a menu bar control, which generally contains a set of top-level
        //     menus.
         MenuBar,
        //
        // Summary:
        //     Identifies a menu item control.
         MenuItem,
        //
        // Summary:
        //     Identifies a pane control.
         Pane,
        //
        // Summary:
        //     Identifies a progress bar control, which visually indicates the progress
        //     of a lengthy operation.
         ProgressBar,
        //
        // Summary:
        //     Identifies a radio button control, which is a selection mechanism allowing
        //     exactly one selected item in a group.
         RadioButton,
        //
        // Summary:
        //     Identifies a scroll bar control, such as a scroll bar in an application window.
         ScrollBar,
        //
        // Summary:
        //     Identifies a separator, which creates a visual division in controls like
        //     menus and toolbars.
         Separator,
        //
        // Summary:
        //     Identifies a slider control.
         Slider,
        //
        // Summary:
        //     Identifies a spinner control.
         Spinner,
        //
        // Summary:
        //     Identifies a split button, which is a button that performs a default action
        //     and can also expand to a list of other possible actions.
         SplitButton,
        //
        // Summary:
        //     Identifies a status bar control.
         StatusBar,
        //
        // Summary:
        //     Identifies a tab control.
         Tab,
        //
        // Summary:
        //     Identifies a tab item control, which represents a page of a tab control.
         TabItem,
        //
        // Summary:
        //     Identifies a table.
         Table,
        //
        // Summary:
        //     Identifies an edit control, such as a text box or rich text box.
         Text,
        //
        // Summary:
        //     Identifies the control in a scrollbar that can be dragged to a different
        //     position.
         Thumb,
        //
        // Summary:
        //     Identifies the caption bar on a window.
         TitleBar,
        //
        // Summary:
        //     Identifies a toolbar, such as the control that contains a set of command
        //     buttons in an application window.
         ToolBar,
        //
        // Summary:
        //     Identifies a tooltip control, an informational window that appears as a result
        //     of moving the pointer over a control or sometimes when tabbing to a control
        //     using the keyboard.
         ToolTip,
        //
        // Summary:
        //     Identifies a tree control.
         Tree,
        //
        // Summary:
        //     Identifies a node in a System.Windows.Automation.ControlType.TreeItem control.
         TreeItem,
        //
        // Summary:
        //     Identifies a window frame, which contains child objects.
         Window,

         Unknown,

         NumericUpDown
    }
}
