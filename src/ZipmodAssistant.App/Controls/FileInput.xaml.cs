using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZipmodAssistant.App.Controls
{
  /// <summary>
  /// Interaction logic for FileInput.xaml
  /// </summary>
  public partial class FileInput : System.Windows.Controls.UserControl
  {
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value",
      typeof(string),
      typeof(FileInput));

    public static readonly DependencyProperty DefaultExtensionProperty = DependencyProperty.Register(
      "DefaultExtension",
      typeof(string),
      typeof(FileInput));

    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
      "Filter",
      typeof(string),
      typeof(FileInput));

    public string Value
    {
      get => (string)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
    }

    public string DefaultExtension
    {
      get => (string)GetValue(DefaultExtensionProperty);
      set => SetValue(DefaultExtensionProperty, value);
    }

    public string Filter
    {
      get => (string)GetValue(FilterProperty);
      set => SetValue(FilterProperty, value);
    }

    private readonly FolderBrowserDialog _dialog = new()
    {
      ShowNewFolderButton = true,
    };

    public FileInput()
    {
      InitializeComponent();
    }

    void BrowseButtonClicked(object sender, RoutedEventArgs e)
    {
      switch (_dialog.ShowDialog())
      {
        case DialogResult.OK:
          Value = _dialog.SelectedPath;
          break;
        default:
          return;
      }
    }
  }
}
