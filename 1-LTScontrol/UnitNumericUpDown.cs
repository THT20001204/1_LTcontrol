using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System;

namespace _1_LTScontrol
{
    public class UnitNumericUpDown : NumericUpDown
    {
        private string _unit = "ms";

        public UnitNumericUpDown()
        {
            // 启用自定义绘制
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                this.Invalidate(); // 更新显示
            }
        }

        // 获取内部TextBox控件
        private TextBox GetTextBox()
        {
            FieldInfo fi = typeof(NumericUpDown).GetField("upDownEdit", BindingFlags.NonPublic | BindingFlags.Instance);
            return fi?.GetValue(this) as TextBox;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!string.IsNullOrEmpty(_unit))
            {
                // 获取内部文本框和按钮区域
                TextBox textBox = GetTextBox();
                int buttonWidth = this.Controls[0].Width; // 按钮宽度

                if (textBox != null)
                {
                    // 计算数值文本宽度
                    Size textSize = TextRenderer.MeasureText(this.Text, this.Font);

                    // 在文本框右侧绘制单位（考虑按钮区域）
                    int unitX = textBox.Left + textSize.Width + 2;
                    int unitY = (this.Height - this.Font.Height) / 2;

                    // 确保单位不进入按钮区域
                    if (unitX + TextRenderer.MeasureText(_unit, this.Font).Width > this.Width - buttonWidth)
                        unitX = this.Width - buttonWidth - TextRenderer.MeasureText(_unit, this.Font).Width;

                    e.Graphics.DrawString(_unit, this.Font, Brushes.Black, unitX, unitY);
                }
                else
                {
                    // 反射失败时的回退方案
                    int textWidth = TextRenderer.MeasureText(this.Text, this.Font).Width;
                    e.Graphics.DrawString(_unit, this.Font, Brushes.Black,
                        textWidth + 5,
                        (this.ClientRectangle.Height - this.Font.Height) / 2);
                }
            }
        }

        protected override void UpdateEditText()
        {
            base.UpdateEditText();

            // 动态调整文本框宽度以容纳单位和数值
            TextBox textBox = GetTextBox();
            if (textBox != null && !string.IsNullOrEmpty(_unit))
            {
                int textWidth = TextRenderer.MeasureText(this.Text, this.Font).Width;
                int unitWidth = TextRenderer.MeasureText(_unit, this.Font).Width;
                int buttonWidth = this.Controls[0].Width;

                // 设置文本框宽度为数值+单位所需宽度，但不超过可用空间
                textBox.Width = Math.Min(
                    textWidth + unitWidth + 5,
                    this.Width - buttonWidth
                );
            }
        }
    }
}