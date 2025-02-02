﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C19_Ex03_LiorFridman_206081085_DorCohen_307993959
{
	public partial class FormToFillText : Form
	{
		public string UserInput { get; set; }

		public bool IsCanceled { get; set; }

		public FormToFillText()
		{
			IsCanceled = true;
			this.InitializeComponent();
		}

		private void m_SubmitBtn_Click(object sender, EventArgs e)
		{
			UserInput = m_TextField.Text;
			IsCanceled = false;
			this.Dispose();
		}

		private void m_CancelBtn_Click(object sender, EventArgs e)
		{
			IsCanceled = true;
			this.Dispose();
		}
	}
}
