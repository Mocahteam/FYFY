using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FYFY_Inspector {
	// Extracted from entitas project: https://github.com/sschmid/Entitas-CSharp
	/// <summary>Display curves to show systems' load</summary>
	internal class SystemsMonitor {

		private float xBorder = 48;
		private float yBorder = 10;
		private int rightLinePadding = -15;
		private string labelFormat = "{0:0.0}";
		private string axisFormat = "{0:0.0}";
		private int gridLines = 1;
		private float axisRounding = 1f;
		private float anchorRadius = 1f;

		private readonly GUIStyle _labelTextStyle;
		private readonly GUIStyle _centeredStyle;
		private readonly Vector3[] _cachedLinePointVerticies;
		private readonly Vector3[] _linePoints;

		/// <summary>Constructor</summary>
		internal SystemsMonitor(int dataLength) {
			_labelTextStyle = new GUIStyle(GUI.skin.label);
			_labelTextStyle.alignment = TextAnchor.UpperRight;
			_centeredStyle = new GUIStyle();
			_centeredStyle.alignment = TextAnchor.UpperCenter;
			_centeredStyle.normal.textColor = Color.white;
			_linePoints = new Vector3[dataLength];
			_cachedLinePointVerticies = new [] {
				new Vector3(-1, 1 ,0) * anchorRadius,
				new Vector3(1, 1, 0) * anchorRadius,
				new Vector3(1, -1, 0) * anchorRadius,
				new Vector3(-1, -1, 0) * anchorRadius,
			};
		}

		/// <summary>Draw lines</summary>
		internal void Draw(float[] data1, float[] data2, float[] data3, float height) {
			var rect = GUILayoutUtility.GetRect(EditorGUILayout.GetControlRect().width, height);
			var top = rect.y;
			var floor = rect.y + rect.height - yBorder;
			var availableHeight = floor - top;
			var max = data1.Length != 0 ? Math.Max(data1.Max(), Math.Max(data2.Max(), data3.Max())) : 0f;
			if (max % axisRounding != 0) {
				max = max + axisRounding - (max % axisRounding);
			}
			max = max == 0 ? 1 : max;
			drawGridLines(top, rect.width, availableHeight, max);
			drawAvg(data1, top, floor, rect.width, availableHeight, max, Color.magenta);
			drawLine(data1, floor, rect.width, availableHeight, max, Color.magenta);
			drawAvg(data2, top, floor, rect.width, availableHeight, max, Color.yellow);
			drawLine(data2, floor, rect.width, availableHeight, max, Color.yellow);
			drawAvg(data3, top, floor, rect.width, availableHeight, max, Color.cyan);
			drawLine(data3, floor, rect.width, availableHeight, max, Color.cyan);
		}

		private void drawGridLines(float top, float width, float availableHeight, float max) {
			var handleColor = Handles.color;
			Handles.color = Color.grey;
			var n = gridLines + 1;
			var lineSpacing = availableHeight / n;
			for (int i = 0; i <= n; i++) {
				var lineY = top + (lineSpacing * i);
				Handles.DrawLine(
					new Vector2(xBorder, lineY),
					new Vector2(width - rightLinePadding, lineY)
				);
				GUI.Label(
					new Rect(0, lineY - 8, xBorder - 2, 50),
					string.Format(axisFormat, max * (1f - ((float)i / (float)n))),
					_labelTextStyle
				);
			}
			Handles.color = handleColor;
		}

		private void drawAvg(float[] data, float top, float floor, float width, float availableHeight, float max, Color lineColor) {
			var handleColor = Handles.color;
			Handles.color = lineColor;

			var avg = data.Average();
			var lineY = floor - (availableHeight * (avg / max));
			Handles.DrawLine(
				new Vector2(xBorder, lineY),
				new Vector2(width - rightLinePadding, lineY)
			);
			Handles.color = handleColor;
		}

		private void drawLine(float[] data, float floor, float width, float availableHeight, float max, Color lineColor) {
			var lineWidth = (float)(width - xBorder - rightLinePadding) / data.Length;
			var handleColor = Handles.color;
			var labelRect = new Rect();
			Vector2 newLine;
			bool mousePositionDiscovered = false;
			float mouseHoverDataValue = 0;
			float linePointScale;
			Handles.color = lineColor;
			Handles.matrix = Matrix4x4.identity;
			HandleUtility.handleMaterial.SetPass(0);
			for (int i = 0; i < data.Length; i++) {
				var value = data[i];
				var lineTop = floor - (availableHeight * (value / max));
				newLine = new Vector2(xBorder + (lineWidth * i), lineTop);
				_linePoints[i] = new Vector3(newLine.x, newLine.y, 0);
				linePointScale = 1f;
				if (!mousePositionDiscovered) {
					var anchorPosRadius3 = anchorRadius * 3;
					var anchorPosRadius6 = anchorRadius * 6;
					var anchorPos = newLine - (Vector2.up * 0.5f);
					labelRect = new Rect(anchorPos.x - anchorPosRadius3, anchorPos.y - anchorPosRadius3, anchorPosRadius6, anchorPosRadius6);
					if (labelRect.Contains(Event.current.mousePosition)) {
						mousePositionDiscovered = true;
						mouseHoverDataValue = value;
						linePointScale = 3f;
					}
				}
				Handles.matrix = Matrix4x4.TRS(_linePoints[i], Quaternion.identity, Vector3.one * linePointScale);
				Handles.DrawAAConvexPolygon(_cachedLinePointVerticies);
			}
			Handles.matrix = Matrix4x4.identity;
			Handles.DrawAAPolyLine(2f, data.Length, _linePoints);

			if (mousePositionDiscovered) {
				labelRect.y -= 16;
				labelRect.width += 50;
				labelRect.x -= 25;
				GUI.Label(labelRect, string.Format(labelFormat, mouseHoverDataValue), _centeredStyle);
			}
			Handles.color = handleColor;
		}
	}
}