//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEngine;
using System.Collections.Generic;

namespace SixenseVR
{
    [SelectionBase]
    public class HandControl : MonoBehaviour
    {
        #region Public Members
        public Hand m_hand;

        public Animator m_basicHand;

        public Transform m_bone;
        public Transform m_grabAnchor;
        public Transform m_sensor;

        public SixenseCore.Tracker m_controller = null;
        #endregion

        #region Private Members
        GrabPoint m_grab = null;
        FixedJoint m_joint = null;

        Rigidbody m_held = null;

        Vector3 lastPos;
        Quaternion lastRot;

        Vector3 velocity;
        Vector3 spin;

        Quaternion targetRot;
        Vector3 targetPos;

        Animator m_AvatarAnimator;

        bool m_wantsToPoint = false;
        bool m_forced = false;

        // keep track of objects that are currently between our fingers, and could be grabbed
        protected HashSet<Collider> tracking = new HashSet<Collider>();

        const float c_momentumRate = 1f;
        const float c_grabRange = 0.03f;    // spherical region about the grab anchor to grab objects
        const float c_backHandRangeMM = 20; // distance behind the hand that we check for grabbing
        #endregion

        #region Properties
        public Rigidbody Holding
        {
            get { return m_held; }
        }

        public Animator AvatarAnimator
        {
            set { m_AvatarAnimator = value; }
        }

        public bool WantsToPoint
        {
            set { m_wantsToPoint = value; }
            get { return m_wantsToPoint; }
        }
        #endregion

        #region Public Methods
        public void Grab(Rigidbody r, GrabPoint g = null, bool forced = false)
        {
            if (r == null)
                return;

            //Debug.Log("Grab " + r.gameObject);

            m_wantsToPoint = false; // stop pointing when grabbing

            if (g == null)
            {
                m_held = r;
                m_joint = transform.gameObject.AddComponent<FixedJoint>();

                m_joint.autoConfigureConnectedAnchor = true;
                m_joint.connectedBody = m_held;

                return;
            }

            m_held = r;
            m_grab = g;
            m_grab.HandControl = this;

            if (m_AvatarAnimator != null)
            {
                m_AvatarAnimator.SetBool("Drop" + (m_hand == Hand.RIGHT ? "Right" : "Left"), false);
                m_AvatarAnimator.SetBool(m_grab.m_animationTransition + (m_hand == Hand.RIGHT ? "Right" : "Left"), true);
            }

            m_joint = transform.gameObject.AddComponent<FixedJoint>();

            m_held.isKinematic = true;

            Quaternion bindRot = Quaternion.Inverse(m_grab.GrabRotation(m_hand)) * r.transform.rotation;

            Vector3 pos = r.transform.position;
            Quaternion rot = r.transform.rotation;

            r.transform.rotation = m_sensor.rotation * bindRot;
            r.transform.position += m_sensor.position - m_grab.GrabPosition(m_hand);

            m_held.transform.parent = transform;

            targetPos = r.transform.localPosition;
            targetRot = r.transform.localRotation;

            if (!forced)
            {
                r.transform.position = pos;
                r.transform.rotation = rot;
            }
            else
            {
                m_AvatarAnimator.SetBool("Drop" + (m_hand == Hand.RIGHT ? "Right" : "Left"), false);
            }

            m_forced = forced;
        }

        public void Drop()
        {
            if (m_held == null)
                return;

            m_held.transform.parent = null;
            m_held.isKinematic = false;

            if (m_grab != null)
            {
                if (m_basicHand.gameObject.activeSelf)
                {
                    m_basicHand.SetBool(m_grab.m_animationTransition, false);
                    m_basicHand.SetBool("Fist", true);
                }

                else if (m_AvatarAnimator != null)
                {
                    m_AvatarAnimator.SetBool(m_grab.m_animationTransition + (m_hand == Hand.RIGHT ? "Right" : "Left"), false);
                    m_AvatarAnimator.SetBool("Drop" + (m_hand == Hand.RIGHT ? "Right" : "Left"), true);
                }

                m_grab.HandControl = null;
            }
            else if (m_joint != null)
            {
                Destroy(m_joint);
            }

            if (!float.IsNaN(velocity.x) && !float.IsNaN(velocity.y) && !float.IsNaN(velocity.z))
                m_held.AddForce(velocity, ForceMode.VelocityChange);

            if (!float.IsNaN(spin.x) && !float.IsNaN(spin.y) && !float.IsNaN(spin.z))
                m_held.AddTorque(spin, ForceMode.VelocityChange);

            m_held = null;
            m_joint = null;
            m_grab = null;
            m_forced = false;
        }
        #endregion

        #region Script Events
        void Start()
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
        }

        void OnTriggerEnter(Collider c)
        {
            tracking.Add(c);
        }
        void OnTriggerExit(Collider c)
        {
            if (tracking.Contains(c))
                tracking.Remove(c);
        }

        void OnDisable()
        {
            tracking.Clear();
        }

        void FixedUpdate()
        {
            if (m_forced)
                return;

            if (m_held != null)
            {
                velocity = (m_held.worldCenterOfMass - lastPos) / Time.fixedDeltaTime;

                Quaternion rotDelta = m_held.rotation * Quaternion.Inverse(lastRot);
                float theta;
                Vector3 axis;
                rotDelta.ToAngleAxis(out theta, out axis);
                spin = Vector3.Lerp(spin, axis.normalized * theta / Time.fixedDeltaTime, Mathf.Clamp01(Time.fixedDeltaTime * c_momentumRate));

                lastPos = m_held.worldCenterOfMass;
                lastRot = m_held.rotation;
            }

            if (m_grab != null && m_held != null)
            {
                m_held.transform.localPosition = Vector3.Lerp(m_held.transform.localPosition, targetPos, Time.fixedDeltaTime * 10);
                m_held.transform.localRotation = Quaternion.Slerp(m_held.transform.localRotation, targetRot, Time.fixedDeltaTime * 10);
            }
        }

        void LateUpdate()
        {
            if (m_forced)
                return;

            if (m_bone == null) // we have no body, so turn on the disembodied hands
            {
                m_bone = transform;
                m_basicHand.gameObject.SetActive(true);
            }
            else if (m_bone.GetComponent<Collider>() != null)
            {
                m_bone.GetComponent<Collider>().enabled = (m_held == null);
            }

            m_AvatarAnimator.SetBool("Drop" + (m_hand == Hand.RIGHT ? "Right" : "Left"), false);

            if (m_basicHand.gameObject.activeSelf)
                m_basicHand.SetBool("Fist", true);

            if (m_controller == null)
            {
                //if (m_basicHand.gameObject.activeSelf)
                //    m_basicHand.SetFloat("FistAmount", 0);

                //m_controller = SixenseInput.Controllers[m_solver.GetTrackerIndex(m_hand)];  // might not return a value at first
            }
            else
            {
                if (m_basicHand.gameObject.activeSelf)
                    m_basicHand.SetFloat("FistAmount", Holding != null ? 1 : m_controller.Trigger, 0.1f, Time.deltaTime);

                if (m_held == null)
                {
                    GrabPoint closest = null;
                    Rigidbody closestCollider = null;
                    foreach (var c in tracking)
                    {
                        if (!c.enabled)
                        {
                            tracking.Remove(c);
                            return;
                        }

                        if (c.GetComponent<Rigidbody>() == null || c.GetComponent<Rigidbody>().isKinematic)
                            continue;

                        var grabs = c.gameObject.GetComponentsInChildren<GrabPoint>();

                        float minDist = float.MaxValue;
                        foreach (var g in grabs)
                        {
                            float dist = Vector3.Distance(m_grabAnchor.position, g.Anchor.position);
                            float angle;

                            if(g.m_radialSymmetry)
                            {
                                Quaternion grip = Quaternion.LookRotation(g.Anchor.forward, m_grabAnchor.up);
                                angle = Quaternion.Angle(m_grabAnchor.rotation, grip);
                            }
                            else 
                            { 
                                angle = Quaternion.Angle(m_grabAnchor.rotation, g.Anchor.rotation);
                            }

                            if (!g.HasInput &&
                                    (m_controller.GetButtonDown(SixenseCore.Buttons.TRIGGER) || m_controller.GetButtonDown(SixenseCore.Buttons.BUMPER)) &&
                                    (g.m_handedness == Hand.UNKNOWN || g.m_handedness == (Hand)m_hand) &&
                                    dist <= g.m_grabRange &&
                                    (g.m_grabMaxAngle >= 180 || angle <= g.m_grabMaxAngle))
                            {
                                // don't grab things that are behind your hand
                                if (m_hand == Hand.LEFT && transform.InverseTransformPoint(g.Anchor.position).x < -c_backHandRangeMM)
                                    continue;
                                if (m_hand == Hand.RIGHT && transform.InverseTransformPoint(g.Anchor.position).x > c_backHandRangeMM)
                                    continue;

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    closest = g;
                                    closestCollider = c.GetComponent<Rigidbody>();
                                }
                            }
                        }

                        if (c.GetComponent<UnityEngine.Joint>() != null)
                            continue;

                        if (closestCollider == null && !c.isTrigger && m_controller.GetButtonDown(SixenseCore.Buttons.TRIGGER))
                        {
                            closestCollider = c.GetComponent<Rigidbody>();
                        }
                    }

                    Grab(closestCollider, closest);
                }
                else if (m_grab != null)   // grips last until bumper is hit
                {
                    if (m_basicHand.gameObject.activeSelf)
                    {
                        m_basicHand.SetBool("Fist", false);
                        m_basicHand.SetBool(m_grab.m_animationTransition, true);
                    }

                    if ((!m_grab.m_dropOnRelease && m_controller.GetButtonDown(SixenseCore.Buttons.BUMPER)) || (m_controller.DeviceIndex > -1 && m_grab.m_dropOnRelease && !m_controller.GetButton(SixenseCore.Buttons.TRIGGER)))
                    {
                        Drop();
                    }
                }
                else if (m_joint != null && !m_controller.GetButton(SixenseCore.Buttons.TRIGGER))    // non-grips require you to hold the trigger
                {
                    Rigidbody c = m_held.GetComponent<Rigidbody>();

                    var grabs = m_held.gameObject.GetComponentsInChildren<GrabPoint>();

                    Drop();

                    GrabPoint closest = null;

                    float minDist = float.MaxValue;

                    foreach (var g in grabs)
                    {
                        float dist = Vector3.Distance(m_grabAnchor.position, g.Anchor.position);
                        float angle;

                        if (g.m_radialSymmetry)
                        {
                            Quaternion grip = Quaternion.LookRotation(g.Anchor.forward, m_grabAnchor.up);
                            angle = Quaternion.Angle(m_grabAnchor.rotation, grip);
                        }
                        else
                        {
                            angle = Quaternion.Angle(m_grabAnchor.rotation, g.Anchor.rotation);
                        }

                        if (!g.HasInput && !g.m_dropOnRelease &&
                            dist <= g.m_grabRange * 2 &&
                            (g.m_grabMaxAngle * 2 >= 180 || angle <= g.m_grabMaxAngle * 2))
                        {
                            if (dist < minDist)
                            {
                                minDist = dist;
                                closest = g;
                            }
                        }
                    }

                    if (closest != null)
                        Grab(c, closest);
                }

                if (m_basicHand.gameObject.activeSelf)
                    m_basicHand.SetBool("Point", m_wantsToPoint);

                if (m_AvatarAnimator != null)
                    m_AvatarAnimator.SetBool("Point" + (m_hand == Hand.RIGHT ? "Right" : "Left"), m_wantsToPoint);
            }
        }
        #endregion
    }
}
