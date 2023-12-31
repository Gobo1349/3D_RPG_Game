using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
//When you add a script which uses RequireComponent to a GameObject, the required component is automatically added to the GameObject.
//This is useful to avoid setup errors. For example a script might require that a Rigidbody is always added to the same GameObject.
//When you use RequireComponent, this is done automatically, so you are unlikely to get the setup wrong.

[RequireComponent(typeof(MeshRenderer))]

public class MeshCombiner : MonoBehaviour // ������ ��� �������� 3� ������� - ��������� ��������� ������� � ���� 
{
    private MeshRenderer _renderer; // �� ��� ���������� ������ ���������� 
    private Mesh _mesh;

    [SerializeField] 
    private Material _material;

    [SerializeField]
    private string _meshName; 

    [ContextMenu("Combine")] // ������� �������� � ����������� ���� 
    void Combine()
    {
        _renderer = gameObject.GetComponent<MeshRenderer>(); // ���������� � ���������� ������� - ����������� ��������� ���������� 
        _mesh = transform.GetComponent<MeshFilter>().sharedMesh; // ����� ���������� � ���, � ��� 

        // ����� �������� ��� �������� �������� 
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combines = new CombineInstance[meshFilters.Length]; // ��������� ��� �������� ����� - ��� �������� ����, ��� ������������ ����� � �������������� Mesh.CombineMeshes
        int i = 0;
        while(i < meshFilters.Length)
        {
            combines[i].mesh = meshFilters[i].sharedMesh;
            combines[i].transform = meshFilters[i].transform.localToWorldMatrix; // ��������� ���������� ������� ��������, ������� ����� ��������� 
            meshFilters[i].gameObject.active = false; // ��������� ������� 
            i++;
        }

        _mesh = new Mesh();
        _mesh.CombineMeshes(combines); // ��������� ����

        transform.gameObject.active = true;
        gameObject.AddComponent<MeshCollider>();

        _renderer.material = Material.Instantiate(_material);  // ����������� �� ��������� ������ ���������, ������� ������ 

        MeshSaverEditor.SaveMesh(_mesh, _meshName, false, false); // ���������, ��� ���������� � ����� 
    }
}
