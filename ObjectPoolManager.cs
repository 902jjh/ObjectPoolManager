using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private Dictionary<string, GameObject> PoolDict = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> PoolParent = new Dictionary<string, GameObject>();
    private Dictionary<string, int> PoolCount = new Dictionary<string, int>();

    [System.Serializable]
    public class ObjectPool
    {
        [Header("Parent Name")]
        public string Name;

        [System.Serializable]
        public class Data
        {
            public GameObject obj;
            public int count;
        }

        public Data[] data;
    }

    [SerializeField] private ObjectPool[] objectpool;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        InitSetting();
    }

    private void InitSetting()
    {
        for (int i = 0; i < objectpool.Length; i++)
        {
            ObjectPool pools = objectpool[i];
            for (int ii = 0; ii < pools.data.Length; ii++)
            {
                GameObject parent = new GameObject(pools.data[ii].obj.name);
                GameObject child = pools.data[ii].obj;

                parent.transform.parent = this.transform;
                
                PoolDict.Add(parent.name, pools.data[ii].obj);
                PoolParent.Add(parent.name, parent);
                PoolCount.Add(parent.name, pools.data[ii].count);

                int count = pools.data[ii].count;
                
                for (int iii = 0; iii < count; iii++)
                {
                    GameObject pool = SpawnObjectPool(parent);
                    pool.SetActive(false);
                    //UnUseObjectPool(pool);
                    // 위에껀 혹시 모르니까 남겨둠 나중에 Bullet한테 맞춰줬던게 문제될 수 있으니까
                    // Bullet의 OnDeSpawn GameObject effect관련
                }
            }
        }
    }

    private GameObject SpawnObjectPool(GameObject pool)
    {
        string name = pool.name;
        GameObject obj = Instantiate(PoolDict[name], PoolParent[name].transform);
        return obj;
    }

    private GameObject SpawnObjectPool(string name)
    {
        GameObject obj = Instantiate(PoolDict[name], PoolParent[name].transform);
        return obj;
    }

    private void DeSpawnObjectPool(GameObject pool)
    {
        Destroy(pool.transform.gameObject);
    }

    private void UseObjectPool(GameObject pool)
    {
        pool.SetActive(true);
        pool.GetComponent<IPoolable>()?.OnSpawn();
    }

    private void UnUseObjectPool(GameObject pool)
    {
        pool.SetActive(false);
        pool.GetComponent<IPoolable>()?.OnDeSpawn();
    }

    public GameObject Get(GameObject pool)
    {
        // 새로 만들지 아니면 기존에 있던것을 재활용할지
        //int count = PoolCount[pool.name];
        GameObject parent = PoolParent[pool.name];

        GameObject obj = null;
        bool IsCreate = true;

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject item = parent.transform.GetChild(i).gameObject;
            if (item.gameObject.activeSelf == false) // 생성 말고 재활용 가능하면
            {
                IsCreate = false;
                obj = item.gameObject;
                break;
            }
        }

        if (IsCreate)
        {
            obj = SpawnObjectPool(pool);
            obj.GetComponent<IPoolable>()?.OnSpawn();
        }
        else
            UseObjectPool(obj);

        return obj;

    }
    public GameObject Get(string name)
    {

        if(PoolDict.Where(x => x.Key == name).Any() is false)
        {
            string fileName = $"'{this.name}'";
            Debug.Log($"해당 '{name}' 이란 이름을 가진 오브젝트는 {fileName}에 없습니다. \n이름을 다시 확인하시거나 {fileName}에 오브젝트를 추가해 주세요");
            return null;
        }

        GameObject parent = PoolParent[name];

        GameObject obj = null;
        bool IsCreate = true;

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject item = parent.transform.GetChild(i).gameObject;
            if (item.gameObject.activeSelf == false)
            {
                IsCreate = false;
                obj = item.gameObject;
                break;
            }
        }

        if (IsCreate)
        {
            obj = SpawnObjectPool(name);
            obj.GetComponent<IPoolable>()?.OnSpawn();
        }
        else
            UseObjectPool(obj);

        return obj;
    }

    public void Release(GameObject pool)
    {
        string obj = PoolDict?[pool.transform.parent.name]?.name;
        int defaultcount = PoolCount[obj];
        int childcount = PoolParent[obj].transform.childCount;

        if(defaultcount >= childcount)
            UnUseObjectPool(pool);
        else
            DeSpawnObjectPool(pool);
    }
}
