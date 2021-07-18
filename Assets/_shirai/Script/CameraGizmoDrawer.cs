using UnityEngine;
using System.Collections;

/*
 * ��I�����ɂ��J�����̃r���[�͈͂�\������
 */

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraGizmoDrawer : MonoBehaviour
{
    public Color gizmosColor = Color.gray;

    void OnDrawGizmos()
    {
        float fov = this.GetComponent<Camera>().fieldOfView;
        float size = this.GetComponent<Camera>().orthographicSize;
        float max = this.GetComponent<Camera>().farClipPlane;
        float min = this.GetComponent<Camera>().nearClipPlane;
        float aspect = this.GetComponent<Camera>().aspect;

        Color tempColor = Gizmos.color;
        Gizmos.color = gizmosColor;

        Matrix4x4 tempMat = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, new Vector3(aspect, 1.0f, 1.0f));

        if (this.GetComponent<Camera>().orthographic)
        {
            //OrthoGraphic�J�����ݒ�
            Gizmos.DrawWireCube(new Vector3(0.0f, 0.0f, ((max - min) / 2.0f) + min), new Vector3(size * 2.0f, size * 2.0f, max - min));
        }
        else
        {
            //Perspective�J�����ݒ�
            Gizmos.DrawFrustum(Vector3.zero, fov, max, min, 1.0f);
        }

        Gizmos.color = tempColor;
        Gizmos.matrix = tempMat;
    }
}