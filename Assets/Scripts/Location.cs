using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Положительное направление по X: право
// Положительное направление по Y: низ
public class Location : MonoBehaviour
{
    public float sellSize = 1;


    // Добавляет объект на сетку уровня.
    // Компонент Transform определяет положение объекта относительно центра клетки.
    public GameObject AddObject(Vector2Int pos, GameObject prefab)
    {
        var obj = Instantiate(prefab);
        ToWorldCoords(new Vector2Int(), pos, obj);
        var cell = obj.AddComponent<Cell>();
        cell.x = pos.x;
        cell.y = pos.y;
        Library.GetOrCreate(cells, pos).Add(obj);
        return obj;
    }

    // Удаляет объект, находящийся на сетке уровня
    public void RemoveObject(GameObject obj)
    {
        var pos = obj.GetComponent<Cell>().ToVec();

        var list = cells[pos];
        list.Remove(obj);
        Destroy(obj);
        if (list.Count == 0)
        {
            cells.Remove(pos);
        }
    }

    // Мгновнно перемещает с клетки на клетку
    public void MoveObject(GameObject obj, Cell cell, Vector2Int to)
    {
        var from = cell.ToVec();

        if (!cells.TryGetValue(from, out var fromObjects))
        {
            throw new Exception("Cell " + from + " doesn't exist");
        }

        var toObjects = Library.GetOrCreate(cells, to);

        if (!fromObjects.Remove(obj))
        {
            throw new Exception("Object in cell " + from + " doesn't exist");
        }

        ToWorldCoords(from, to, obj);
        cell.x = to.x;
        cell.y = to.y;
        toObjects.Add(obj);
    }

    // Проверяет, есть ли объект со всеми нужными компоентами в клетке
    public bool Has(Vector2Int pos, List<Type> components)
    {
        if (cells.TryGetValue(pos, out var objects))
        {
            foreach (var obj in objects)
            {
                var hasAll = true;
                foreach (var component in components)
                {
                    if (obj.GetComponent(component) == null)
                    {
                        hasAll = false;
                        break;
                    }
                }

                if (hasAll)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Проверяет, есть ли объект с нужным компоентом в клетке
    public bool Has(Vector2Int pos, Type component)
    {
        var a = new List<Type> {component};
        return Has(pos, a);
    }

    // Проверяет, есть ли объекты в клетке
    public bool Has(Vector2Int pos)
    {
        return cells.ContainsKey(pos);
    }

    // Возвращает объекты в клетке, содержащие все перечисленные компоненты
    public List<GameObject> Query(Vector2Int pos, List<Type> components)
    {
        var result = new List<GameObject>();
        if (cells.TryGetValue(pos, out var objects))
        {
            foreach (var obj in objects)
            {
                var hasAll = true;
                foreach (var component in components)
                {
                    if (obj.GetComponent(component) == null)
                    {
                        hasAll = false;
                        break;
                    }
                }

                if (hasAll)
                {
                    result.Add(obj);
                }
            }
        }
        else
        {
            throw new Exception("Cell " + pos + " doesn't exist");
        }

        return result;
    }

    // Возвращает объекты в клетке, содержащий компонент 
    public List<GameObject> Query(Vector2Int pos, Type component)
    {
        var a = new List<Type> {component};
        return Query(pos, a);
    }

    // Возвращает все объекты в клетке 
    public List<GameObject> Query(Vector2Int pos)
    {
        var a = new List<Type>();
        return Query(pos, a);
    }

    // Возвращает объекты на прямоугольной площади, содержащие все перечисленные компоненты
    public List<GameObject> QueryArea(Vector2Int leftTop, Vector2Int rightBottom, List<Type> components)
    {
        var result = new List<GameObject>();
        Assert.IsTrue(leftTop.x < rightBottom.x && leftTop.y < rightBottom.y);

        for (var i = leftTop.y; i < rightBottom.y; i++)
        {
            for (var j = leftTop.x; j < rightBottom.x; j++)
            {
                result.AddRange(Query(new Vector2Int(j, i), components));
            }
        }

        return result;
    }

    // Возвращает объекты на прямоугольной площади, содержащие указанный компонент
    public List<GameObject> QueryArea(Vector2Int leftTop, Vector2Int rightBot, Type component)
    {
        var a = new List<Type> {component};
        return QueryArea(leftTop, rightBot, a);
    }

    // Возвращает все объекты на прямоугольной площади
    public List<GameObject> QueryArea(Vector2Int leftTop, Vector2Int rightBot)
    {
        var a = new List<Type>();
        return QueryArea(leftTop, rightBot, a);
    }

    private void ToWorldCoords(Vector2Int oldPos, Vector2Int newPos, GameObject obj)
    {
        var gridFrom = (Vector2) oldPos * sellSize;
        var gridTo = (Vector2) newPos * sellSize;
        var position = obj.transform.position;
        obj.transform.position = new Vector3(
            gridTo.x + position.x - gridFrom.x, position.y, gridTo.y + position.z - gridFrom.y);
    }

    private Dictionary<Vector2, List<GameObject>> cells = new Dictionary<Vector2, List<GameObject>>();
}