using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
//When you add a script which uses RequireComponent to a GameObject, the required component is automatically added to the GameObject.
//This is useful to avoid setup errors. For example a script might require that a Rigidbody is always added to the same GameObject.
//When you use RequireComponent, this is done automatically, so you are unlikely to get the setup wrong.

[RequireComponent(typeof(MeshRenderer))]

public class MeshCombiner : MonoBehaviour // скрипт дл€ батчинга 3д моделей - соедин€ем несколько моделей в одну 
{
    private MeshRenderer _renderer; // на эти переменные вешаем компоненты 
    private Mesh _mesh;

    [SerializeField] 
    private Material _material;

    [SerializeField]
    private string _meshName; 

    [ContextMenu("Combine")] // команда по€витс€ в контекстном меню 
    void Combine()
    {
        _renderer = gameObject.GetComponent<MeshRenderer>(); // обращаемс€ к компоненту объекта - присваеваем компонент переменной 
        _mesh = transform.GetComponent<MeshFilter>().sharedMesh; // можно обращатьс€ и так, и так 

        // нужно получить все дочерние элементы 
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combines = new CombineInstance[meshFilters.Length]; // структура дл€ описани€ мешей - дл€ описани€ того, как объедин€ютс€ сетки с использованием Mesh.CombineMeshes
        int i = 0;
        while(i < meshFilters.Length)
        {
            combines[i].mesh = meshFilters[i].sharedMesh;
            combines[i].transform = meshFilters[i].transform.localToWorldMatrix; // локальные координаты каждого элемента, которые будем сращивать 
            meshFilters[i].gameObject.active = false; // выключаем элемент 
            i++;
        }

        _mesh = new Mesh();
        _mesh.CombineMeshes(combines); // сращиваем меши

        transform.gameObject.active = true;
        gameObject.AddComponent<MeshCollider>();

        _renderer.material = Material.Instantiate(_material);  // накладываем на финальный объект результат, который укажем 

        MeshSaverEditor.SaveMesh(_mesh, _meshName, false, false); // сохран€ем, что получилось в ассет 
    }
}
