﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZipmodAssistant.App.Converters
{
  public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
  {
    public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) { }
  }
}
