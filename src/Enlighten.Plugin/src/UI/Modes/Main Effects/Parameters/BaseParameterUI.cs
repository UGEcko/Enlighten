using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Enlighten.UI
{
	public abstract class BaseParameterUI : MonoBehaviour, IPointerDownHandler
	{
		protected int m_selectedKeyframeIndex;
		public int SelectedKeyframeIndex => m_selectedKeyframeIndex;
		private GameObject m_selectionIndicator;
		
		public void Initialize()
		{
			m_onUIChanged.AddListener(m_onInteracted.Invoke);
			m_selectionIndicator = transform.Find("Selection Indicator").gameObject;
			HideSelectionIndicator();
		}

		public void HideSelectionIndicator()
		{
			m_selectionIndicator.SetActive(false);
		}

		public void ShowSelectionIndicator()
		{
			m_selectionIndicator.SetActive(true);
		}

		public abstract void UpdateUI();
		public UnityEvent m_onUIChanged = new UnityEvent();
		public UnityEvent m_onInteracted = new UnityEvent();

		public void SetActiveKeyframeIndex(int index)
		{
			m_selectedKeyframeIndex = index;
			UpdateUI();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			m_onInteracted.Invoke();
		}
	}
}
