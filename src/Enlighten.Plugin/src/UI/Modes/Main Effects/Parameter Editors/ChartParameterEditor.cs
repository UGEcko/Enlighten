using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enlighten.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Enlighten.UI
{
	internal abstract class ChartParameterEditor<T> : BaseParameterEditor, IPointerDownHandler
	{
		private RectTransform m_pointsParent;
		private ChartCurveRenderer m_curveRenderer;
		private GameObject m_selectedKeyframeIndicator;
		private BundleLoading.Assets m_assets;
		private GenericParameter<T> m_parameter;
		private ChartKeyframe[] m_keyframes;
		private int m_currentSelectedIndex = -1;

		private const float DOUBLE_CLICK_DURATION = 0.5f;
		private Coroutine m_clickedCoroutine;

		protected float m_chartLowBound = 0f;
		protected float m_chartHighBound = 1f;

		public void Initialize(BundleLoading.Assets assets)
		{
			m_assets = assets;

			Transform graph = transform.Find("Graph");
			m_pointsParent = graph.Find("Points").GetComponent<RectTransform>();
			m_curveRenderer = graph.Find("Curves").gameObject.AddComponent<ChartCurveRenderer>();
			m_selectedKeyframeIndicator = graph.Find("SelectedKeyframeIndicator").gameObject;

			m_onKeyframeSelected.AddListener(OnKeyframeSelected);
			m_onKeyframeChanged.AddListener(OnKeyframeChanged);
		}

		private void OnKeyframeSelected(int index)
		{
			m_currentSelectedIndex = index;
			UpdateSelectedKeyframeIndicator(index);
		}

		private void OnKeyframeChanged(int index)
		{
			UpdateSelectedKeyframeIndicator(index);
		}

		private void UpdateSelectedKeyframeIndicator(int index)
		{
			m_selectedKeyframeIndicator.transform.position = m_keyframes[index].transform.position;
		}

		public void OpenParameter(GenericParameter<T> parameter)
		{
			m_parameter = parameter;
		}

		private IEnumerable<ChartKeyframe> GetKeyframes()
		{
			for (int i = 0; i < m_parameter.Count; i++)
			{
				ChartKeyframe keyframe = Instantiate(m_assets.m_pointPrefab, m_pointsParent).AddComponent<ChartKeyframe>();
				int tempIndex = i;
				keyframe.onClicked += () => m_onKeyframeSelected.Invoke(tempIndex);
				keyframe.onMoved += position => OnKeyframeMove(tempIndex, position);
				yield return keyframe;
			}
		}

		protected void UpdateKeyframePositions()
		{
			for (int i = 0; i < m_parameter.Count; i++)
			{
				GenericParameter<T>.Keyframe data = m_parameter[i];
				ChartKeyframe obj = m_keyframes[i];
				float chartX = data.m_time;
				float chartY = ValueToChartYPosition(data.m_value);
				Vector2 screenPos = ChartToLocalPosition(new Vector2(chartX, chartY));
				obj.transform.localPosition = screenPos;
			}
		}

		protected Vector2 LocalToChartPosition(Vector2 localPosition)
		{
			Vector2 chartPosition = localPosition - m_pointsParent.rect.min;
			chartPosition /= m_pointsParent.rect.size;
			chartPosition.y += m_chartLowBound;
			chartPosition.y *= m_chartHighBound - m_chartLowBound;
			return chartPosition;
		}

		protected Vector2 ChartToLocalPosition(Vector2 chartPosition)
		{
			Vector2 localPosition = chartPosition;
			localPosition.y /= m_chartHighBound - m_chartLowBound;
			localPosition.y -= m_chartLowBound;
			localPosition *= m_pointsParent.rect.size;
			return m_pointsParent.rect.min + localPosition;
		}

		private Vector2 ScreenToLocalPosition(Vector2 screenPosition)
		{
			Vector2 localPosition = m_pointsParent.InverseTransformPoint(screenPosition);
			localPosition = m_pointsParent.ClampPointInside(localPosition);
			return localPosition;
		}

		private GenericParameter<T>.Keyframe ChartPositionToKeyframe(Vector2 position)
		{
			return new GenericParameter<T>.Keyframe
			{
				m_time = position.x,
				m_value = ChartYPositionToValue(position.y)
			};
		}

		protected abstract T ChartYPositionToValue(float y);
		protected abstract float ValueToChartYPosition(T value);

		private void OnKeyframeMove(int index, Vector2 screenPosition)
		{
			Vector2 localPosition = ScreenToLocalPosition(screenPosition);
			Vector2 chartPosition = LocalToChartPosition(localPosition);
			chartPosition.y = ValueToChartYPosition(ChartYPositionToValue(chartPosition.y));

			m_keyframes[index].transform.localPosition = ChartToLocalPosition(chartPosition);
			m_parameter[index] = ChartPositionToKeyframe(chartPosition);

			m_onKeyframeChanged.Invoke(index);
			RedrawCurves();
		}

		public void RedrawKeyframes()
		{
			foreach (Transform child in m_pointsParent)
			{
				Destroy(child.gameObject);
			}
			m_keyframes = GetKeyframes().ToArray();
			UpdateKeyframePositions();
		}

		public override void RedrawCompletely()
		{
			RedrawKeyframes();
			RedrawCurves();
		}

		private void RedrawCurves()
		{
			GenericParameter<T>.Keyframe[] keyframes = m_parameter.SortedKeyframes;
			IEnumerable<Vector2> points = GetCurveLocalPoints(m_parameter, keyframes);
			m_curveRenderer.CalculateCurve(points, 1, m_pointsParent.rect.height);
		}

		protected abstract IEnumerable<Vector2> GetCurveLocalPoints(GenericParameter<T> parameter, GenericParameter<T>.Keyframe[] keyframes);

		public void OnPointerDown(PointerEventData eventData)
		{
			HandleDoubleClickCheck(eventData);
		}

		private void HandleDoubleClickCheck(PointerEventData eventData)
		{
			if (m_clickedCoroutine != null)
			{
				StopCoroutine(m_clickedCoroutine);
				AddKeyframe(eventData.position);
				m_clickedCoroutine = null;
			}
			else
			{
				m_clickedCoroutine = StartCoroutine(ClickedTimer());
			}
		}

		private IEnumerator ClickedTimer()
		{
			yield return new WaitForSeconds(DOUBLE_CLICK_DURATION);
			m_clickedCoroutine = null;
		}

		private void AddKeyframe(Vector2 screenPosition)
		{
			Vector2 localPosition = ScreenToLocalPosition(screenPosition);
			Vector2 chartPosition = LocalToChartPosition(localPosition);
			GenericParameter<T>.Keyframe keyframe = ChartPositionToKeyframe(chartPosition);
			m_parameter.Add(keyframe);
			RedrawCompletely();
			m_onKeyframeSelected.Invoke(m_parameter.Count - 1);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
			{
				DeleteSelectedKeyframe();
			}
		}

		private void DeleteSelectedKeyframe()
		{
			if (m_currentSelectedIndex == -1)
				return;

			if (m_parameter.Count < 2)
				return;

			m_parameter.RemoveAt(m_currentSelectedIndex);
			RedrawCompletely();
			m_onKeyframeSelected.Invoke(m_parameter.Count - 1);
		}
	}
}
