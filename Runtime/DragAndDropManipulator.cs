using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DragAndDropManipulator : PointerManipulator
    {
        private bool Enabled { get; set; }

        private VisualElement Root { get; }

        private VisualElement m_StartContainer;
        private VisualElement m_SlotsContainer;
        private VisualElement m_ClosestOverlappingSlot;
        private VisualElement m_ShadowDragElement;
        private VisualElement m_ShadowDragElementParent;
        private Vector3 parentStartPos;
        private Vector2 parentMouseDelta;
        private DragElementWithImage m_targetUserData;

        private bool m_isSingletonSlot;
        private VisualElement m_displacedElement;
        public DragAndDropManipulator(VisualElement target, bool isSingletonSlot, VisualElement slotsContainer = null)
        {
            this.target = target;
            m_targetUserData = target.userData as DragElementWithImage;
            Root = target.parent;
            if (slotsContainer == null)
            {
                m_SlotsContainer = target.parent.parent.parent.parent.Q<VisualElement>("image-container");
            }
            else
            {
                m_SlotsContainer = slotsContainer;
            }
            if (target.GetType() == typeof(Label))
            {
                m_ShadowDragElement = new Label();
                (m_ShadowDragElement as Label).text = (target as Label)?.text;
            }
            else
            {
                m_ShadowDragElement = new Image();
                (m_ShadowDragElement as Image).image = (target as Image).image;
            }
            m_ShadowDragElement.SetEnabled(false);
            m_ShadowDragElement.AddToClassList("draggableElement");
            m_ShadowDragElement.name = "shadow";
            m_ShadowDragElementParent = new VisualElement();
            m_ShadowDragElementParent.Add(m_ShadowDragElement);
            m_ShadowDragElementParent.AddToClassList("draggableElementContainer");
            m_isSingletonSlot = isSingletonSlot;



        }

        protected override void RegisterCallbacksOnTarget()
        {
            // Register the four callbacks on target.
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            // Un-register the four callbacks from target.
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }


        // This method stores the starting position of target and the pointer,
        // makes target capture the pointer, and denotes that a drag is now in progress.
        private void PointerDownHandler(PointerDownEvent evt)
        {
            m_StartContainer = target.parent.parent;
            target.parent.style.position = Position.Absolute;
            target.parent.BringToFront();
            parentStartPos = target.parent.transform.position;
            // The difference between top left and where the mouse actually clicked
            parentMouseDelta = new Vector2(evt.position.x - target.parent.worldBound.position.x, evt.position.y - target.parent.worldBound.position.y);
            // Because we set the Position to Absolute we need to update the position directly after the click
            UpdateTargetPosition(evt.position);
            target.CapturePointer(evt.pointerId);
            Enabled = true;
            ToggleShowAllFittingZones();
            ActionManager.OnButtonPressed?.Invoke();
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, calculates a new position for target within the bounds of the window.
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (Enabled && target.HasPointerCapture(evt.pointerId))
            {
                UpdateTargetPosition(evt.position);
                UQueryBuilder<VisualElement> overlappingSlots = m_SlotsContainer.Query<VisualElement>(className: "dropZone").Where(OverlapsTarget);
                var lastOverlappingSlot = m_ClosestOverlappingSlot;
                m_ClosestOverlappingSlot = FindClosestSlot(overlappingSlots);
                UpdateShadow(lastOverlappingSlot);
            }
        }

        private void ToggleShowAllFittingZones()
        {
            foreach (var slot in m_SlotsContainer.Query<VisualElement>(className: "dropZone").ToList())
            {
                var slotUserData = slot.userData as DropZoneData;
                if (m_targetUserData != null && slotUserData != null && slotUserData.Group != m_targetUserData.DragGroup)
                {
                    continue;
                }
                slot.ToggleInClassList("highlight-slot");
            }

        }
        private void UpdateTargetPosition(Vector3 position)
        {
            target.parent.transform.position = target.parent.parent.WorldToLocal(position) - parentMouseDelta;
        }

        private void UpdateShadow(VisualElement lastOverlappingSlot)
        {
            if (m_isSingletonSlot)
            {
                // no need to update shadows if the slot did not change
                if (lastOverlappingSlot == m_ClosestOverlappingSlot)
                {
                    return;
                }
                //fix displacedElement and it's shadow
                if (m_displacedElement != null && lastOverlappingSlot != m_ClosestOverlappingSlot)
                {
                    lastOverlappingSlot.Add(m_displacedElement.parent);
                    var displacedElementManipulator = m_displacedElement.parent.userData as DragAndDropManipulator;
                    displacedElementManipulator.m_ShadowDragElementParent.parent.Remove(displacedElementManipulator.m_ShadowDragElementParent);
                    m_displacedElement = null;
                }


                // we are overlapping something so we need to check if there already is an element and if there is, remove it and show a shadow where it will be next
                if (m_ClosestOverlappingSlot != null)
                {
                    m_displacedElement = m_ClosestOverlappingSlot.Q<VisualElement>(className: "draggableElement", name: "draggableElement");
                    if (m_displacedElement == target)
                    { m_displacedElement = null; }
                    if (m_displacedElement != null)
                    {
                        m_ClosestOverlappingSlot.Remove(m_displacedElement.parent);
                        var displacedElementManipulator = m_displacedElement.parent.userData as DragAndDropManipulator;
                        m_StartContainer.Add(displacedElementManipulator.m_ShadowDragElementParent);
                    }
                    m_ClosestOverlappingSlot.Add(m_ShadowDragElementParent);
                }
                else
                {
                    m_StartContainer.Add(m_ShadowDragElementParent);
                }
            }
            else
            {
                if (m_ClosestOverlappingSlot != null)
                {
                    m_ClosestOverlappingSlot.Add(m_ShadowDragElementParent);
                }
                else
                {
                    m_StartContainer.Add(m_ShadowDragElementParent);
                }
            }

        }


        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // If both are true, makes target release the pointer.
        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (Enabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
                ToggleShowAllFittingZones();
                ActionManager.OnButtonPressed?.Invoke();

            }
        }


        // This method checks whether a drag is in progress. If true, queries the root
        // of the visual tree to find all slots, decides which slot is the closest one
        // that overlaps target, and sets the position of target so that it rests on top
        // of that slot. Sets the position of target back to its original position
        // if there is no overlapping slot.
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (Enabled)
            {
                // Reset visuals
                target.parent.style.position = Position.Relative;
                target.parent.transform.position = parentStartPos;
                if (m_ShadowDragElementParent.parent != null)
                {
                    m_ShadowDragElementParent.parent.Add(target.parent);
                    m_ShadowDragElementParent.parent.Remove(m_ShadowDragElementParent);
                }
                // Add displacedElement to the correct place
                if (m_isSingletonSlot && m_displacedElement != null)
                {
                    var displacedElementManipulator = m_displacedElement.parent.userData as DragAndDropManipulator;
                    var displacedShadowElement = displacedElementManipulator.m_ShadowDragElement;
                    displacedShadowElement.parent.parent.Add(m_displacedElement.parent);
                    displacedShadowElement.parent.parent.Remove(displacedShadowElement.parent);
                }
                Enabled = false;
            }
            m_displacedElement = null;
        }

        private bool OverlapsTarget(VisualElement slot)
        {
            return target.worldBound.Overlaps(slot.worldBound);
        }

        private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
        {
            List<VisualElement> slotsList = slots.ToList();
            float bestDistanceSq = float.MaxValue;
            VisualElement closest = null;
            var targetMiddle = new Vector2(target.transform.position.x + (target.worldBound.xMax - target.worldBound.xMin) / 2, target.transform.position.y + (target.worldBound.yMax - target.worldBound.yMin) / 2);
            foreach (VisualElement slot in slotsList)
            {
                var slotUserData = slot.userData as DropZoneData;
                if (m_targetUserData != null && slotUserData != null && slotUserData.Group != m_targetUserData.DragGroup)
                {
                    continue;
                }
                var slotPosition = target.parent.WorldToLocal(slot.parent.LocalToWorld(slot.layout.position));
                var slotMiddle = new Vector2(slotPosition.x + (slot.worldBound.xMax - slot.worldBound.xMin) / 2, slotPosition.y + (slot.worldBound.yMax - slot.worldBound.yMin) / 2);
                var distanceVec = targetMiddle - slotMiddle;
                var distanceSq = distanceVec.sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    closest = slot;
                }
            }
            return closest;
        }
    }
}