using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public float AimingMultiplier = .6f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90f;
        public float MaximumX = 90f;
        public int smoothFrames = 3;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        private List<float> _xRotList;
        private List<float> _yRotList;

        [HideInInspector]
        public bool IsAiming;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;

            _xRotList = GenerateList<float>(smoothFrames + 1, 0f);
            _yRotList = GenerateList<float>(smoothFrames + 1, 0f);

            IsAiming = false;
        }

        private List<T> GenerateList<T>(int size, T value)
        {
            List<T> tempList = new List<T>();

            for(int i = 0; i < size; i++)
            {
                tempList.Add(value);
            }

            return tempList;
        }


        public void LookRotation(Transform character, Transform camera)
        {
            _xRotList.RemoveAt(0);
            _yRotList.RemoveAt(0);

            float tempAimingMultiplier = IsAiming ? AimingMultiplier : 1f;

            _yRotList.Add(Input.GetAxis("Mouse X") * XSensitivity * tempAimingMultiplier);
            _xRotList.Add(Input.GetAxis("Mouse Y") * YSensitivity * tempAimingMultiplier);

            float yRot = GetAverage(_yRotList);
            float xRot = GetAverage(_xRotList);

            m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);


            character.localRotation = m_CharacterTargetRot;
            camera.localRotation = m_CameraTargetRot;

            UpdateCursorLock();
        }

        private float GetAverage(List<float> list)
        {
            float total = 0f;

            foreach(float f in list)
            {
                total += f;
            }

            return total / list.Count;
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
