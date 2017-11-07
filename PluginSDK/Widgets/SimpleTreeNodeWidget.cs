using System;
using System.Drawing;
using System.Collections;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;

namespace WorldWind.NewWidgets
{
	/// <summary>
	/// Summary description for TextLabel.
	/// </summary>
	public class SimpleTreeNodeWidget : TreeNodeWidget
	{
		/// <summary>
		/// Default constructor.  Stub
		/// </summary>
		public SimpleTreeNodeWidget() :base()
		{
		}

		/// <summary>
		/// Constructor that allows passing in a name
		/// </summary>
		/// <param name="name"></param>
		public SimpleTreeNodeWidget(string name) : base(name)
		{
		}

		/// <summary>
		/// Specialized render for tree nodes
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="xOffset">The offset from the left based on how deep this node is nested</param>
		/// <param name="yOffset">The offset from the top based on how many treenodes are above this one</param>
		/// <returns>Total pixels consumed by this widget and its children</returns>
		public override int Render(DrawArgs drawArgs, int xOffset, int yOffset)
		{
			m_ConsumedSize.Height = 0;

			if (m_visible)
			{
				if (!m_isInitialized)
					this.Initialize(drawArgs);

				m_ConsumedSize.Height = NODE_HEIGHT;

				// This value is dynamic based on the number of expanded nodes above this one
				m_location.Y = yOffset;

				// store this value so the mouse events can figure out where the buttons are
				m_xOffset = xOffset;

				// compute the color
				int color = this.Enabled ? m_itemOnColor : m_itemOffColor;

				// create the bounds of the text draw area
				Rectangle bounds = new Rectangle(this.AbsoluteLocation, new System.Drawing.Size(this.ClientSize.Width, NODE_HEIGHT));

				if (m_isMouseOver)
				{
					if (!Enabled)
						color = m_mouseOverOffColor;

					WidgetUtilities.DrawBox(
						bounds.X,
						bounds.Y,
						bounds.Width,
						bounds.Height,
						0.0f,
						m_mouseOverColor,
						drawArgs.device);
				}

				#region Draw arrow

				bounds.X = this.AbsoluteLocation.X + xOffset;
				bounds.Width = NODE_ARROW_SIZE;
				// draw arrow if any children
				if (m_subNodes.Count > 0)
				{
					m_worldwinddingsFont.DrawText(
						null,
						(this.m_isExpanded ? "L" : "A"),
						bounds,
						DrawTextFormat.None,
						color);
				}
				#endregion Draw arrow

				#region Draw checkbox

				bounds.Width = NODE_CHECKBOX_SIZE;
				bounds.X += NODE_ARROW_SIZE;

				// Normal check symbol
				string checkSymbol;
				
				if (m_isRadioButton)
				{
					checkSymbol = this.IsChecked ? "O" : "P";
				}
				else
				{
					checkSymbol = this.IsChecked ? "N" : "F";
				}
				
				m_worldwinddingsFont.DrawText(
					null,
					checkSymbol,
					bounds,
					DrawTextFormat.NoClip,
					color);

				#endregion draw checkbox

				#region Draw name

				// compute the length based on name length 
				// TODO: Do this only when the name changes
				Rectangle stringBounds = drawArgs.defaultDrawingFont.MeasureString(null, Name, DrawTextFormat.NoClip, 0);
				m_size.Width = NODE_ARROW_SIZE + NODE_CHECKBOX_SIZE + 5 + stringBounds.Width;
				m_ConsumedSize.Width = m_size.Width;

				bounds.Y += 2;
				bounds.X += NODE_CHECKBOX_SIZE + 5;
				bounds.Width = stringBounds.Width;

				drawArgs.defaultDrawingFont.DrawText(
					null,
					Name,
					bounds,
					DrawTextFormat.None,
					color);

				#endregion Draw name

				if (m_isExpanded)
				{
					int newXOffset = xOffset + NODE_INDENT;

					for (int i = 0; i < m_subNodes.Count; i++)
					{
						if (m_subNodes[i] is TreeNodeWidget)
						{
							m_ConsumedSize.Height += ((TreeNodeWidget)m_subNodes[i]).Render(drawArgs, newXOffset, m_ConsumedSize.Height);
						}
						else
						{
							System.Drawing.Point newLocation = m_subNodes[i].Location;
							newLocation.Y = m_ConsumedSize.Height;
							newLocation.X = newXOffset;
							m_ConsumedSize.Height += m_subNodes[i].WidgetSize.Height;
							m_subNodes[i].Location = newLocation;
							m_subNodes[i].Render(drawArgs);
							// render normal widgets as a stack of widgets
						}

						// if the child width is bigger than my width save it as the consumed width for widget size calculations
						if (m_subNodes[i].WidgetSize.Width + newXOffset > m_ConsumedSize.Width)
						{
							m_ConsumedSize.Width = m_subNodes[i].WidgetSize.Width + newXOffset ;
						}
					}
				}
			}
			return m_ConsumedSize.Height;
		}
	}
}
