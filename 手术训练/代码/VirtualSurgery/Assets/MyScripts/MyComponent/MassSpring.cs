using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;

public class Direction
{
	float[]p_val = new float[3];
	public Direction(float x=1,float y=0,float z=0)
	{
		SetDir (x, y, z);
	}
	public void SetDir (float x,float y,float z)
	{
		p_val[0]=x;
		p_val[1]=y;
		p_val[2]=z;
		unitization();
	}
	public void SetDir(float []val)
	{
		SetDir(val[0],val[1],val[2]);
	}
	public Direction Reverse()
	{
		this.p_val[0]=-this.p_val[0];
		this.p_val[1]=-this.p_val[1];
		this.p_val[2]=-this.p_val[2];
		return this;
	}
	public static float operator *( Direction dir1,Direction dir2)
	{
		return dir1.p_val[0]*dir2.p_val[0]+dir1.p_val[1]*dir2.p_val[1]+dir1.p_val[2]*dir2.p_val[2];
	}
	public static Direction operator & (Direction dir1,Direction dir2)
	{
		Direction cross=new Direction();
		cross.SetDir(dir1.p_val[1]*dir2.p_val[2]-dir2.p_val[1]*dir1.p_val[2], dir1.p_val[2]*dir2.p_val[0]-dir2.p_val[2]*dir1.p_val[0], dir1.p_val[0]*dir2.p_val[1]-dir2.p_val[0]*dir1.p_val[1]);
		cross.SetDir(dir1.p_val[1]*dir2.p_val[2]-dir2.p_val[1]*dir1.p_val[2], dir1.p_val[2]*dir2.p_val[0]-dir2.p_val[2]*dir1.p_val[0], dir1.p_val[0]*dir2.p_val[1]-dir2.p_val[0]*dir1.p_val[1]);
		return cross;
	}
	public void unitization()
	{
		float one;
		one = (float)Math.Sqrt(Math.Pow(p_val[0],2)+Math.Pow(p_val[1],2)+Math.Pow(p_val[2],2));
		if(one==1) return;
		if(one!=0)
		{
			p_val[0]/=one;
			p_val[1]/=one;
			p_val[2]/=one;
		}
	}
	public  float this[int index]
	{
		get { return p_val[index]; }
		set { this.p_val[index] = value; }
	}
}

public class Complexor
{
	Direction c_dir=new Direction();
	float c_size;
	public Complexor(float x=0,float y=0,float z=0)
	{
		SetCpr(x,y,z);
	}
	public void SetCpr(float x,float y,float z)
	{
		c_dir.SetDir(x,y,z);
		c_size=(float)Math.Sqrt(Math.Pow(x,2)+Math.Pow(y,2)+Math.Pow(z,2));
	}
	public void SetCpr(float []val)
	{
		SetCpr(val[0],val[1],val[2]);
	}
	public void SetCpr(float size,Direction dir)
	{
		this.c_size=size;
		this.c_dir [0] = dir [0];
		this.c_dir [1] = dir [1];
		this.c_dir [2] = dir [2];
		//this.c_dir=dir;
	}
	public float GetSize()
	{
		return c_size;
	}
	public Direction GetDir()
	{
		return c_dir;
	}
	public Complexor unitization()
	{
		Complexor unit=new Complexor();
		unit.SetCpr((float)1.0,c_dir);
		return unit;
	}
	public void Reverse()
	{
		this.c_dir.Reverse();
	}
	public static Complexor operator+(Complexor c1,Complexor c2)
	{
		Complexor sum=new Complexor();
		sum.SetCpr(c1.c_dir[0]*c1.c_size+c2.c_dir[0]*c2.c_size, c1.c_dir[1]*c1.c_size+c2.c_dir[1]*c2.c_size, c1.c_dir[2]*c1.c_size+c2.c_dir[2]*c2.c_size);
		return sum;
	}
	public static Complexor operator-(Complexor c1,Complexor c2)
	{
		Complexor dif=new Complexor();
		dif.SetCpr(c1.c_dir[0]*c1.c_size-c2.c_dir[0]*c2.c_size, c1.c_dir[1]*c1.c_size-c2.c_dir[1]*c2.c_size, c1.c_dir[2]*c1.c_size-c2.c_dir[2]*c2.c_size);
		return dif;
	}
	public static float operator*(Complexor c1,Complexor c2)
	{
		return c1.c_dir[0]*c1.c_size*c2.c_dir[0]*c2.c_size+c1.c_dir[1]*c1.c_size*c2.c_dir[1]*c2.c_size+c1.c_dir[2]*c1.c_size*c2.c_dir[2]*c2.c_size;
	}
	public static Complexor operator*(float c,Complexor com)
	{
		Complexor mul=new Complexor();
		mul.c_size=com.c_size*c;
		mul.c_dir=com.c_dir;
		return mul;
	}
	public float this[int index]
	{
		get{return c_dir[index];}
		set{this.c_dir[index]=value;}
	}
}

public class VPoint
{
	float []v_point=new float[3];
	public VPoint(float x=0,float y=0,float z=0)
	{
		SetPoint(x,y,z);
	}
	public void SetPoint(float x,float y,float z)
	{
		this.v_point[0]=x;
		this.v_point[1]=y;
		this.v_point[2]=z;
	}
	public void SetPoint(float []p)
	{
		SetPoint(p[0],p[1],p[2]);
	}
	public static Complexor operator-(VPoint p1,VPoint p2)
	{
		Complexor c=new Complexor();
		c.SetCpr(p1.v_point[0]-p2.v_point[0],p1.v_point[1]-p2.v_point[1],p1.v_point[2]-p2.v_point[2]);
		return c;
	}
	public static VPoint operator +(VPoint p,Complexor c)
	{
		p.v_point[0]+=c[0]*c.GetSize();
		p.v_point[1]+=c[1]*c.GetSize();
		p.v_point[2]+=c[2]*c.GetSize();
		return p;
	}
	public float this[int index]
	{
		get{return v_point[index];}
		set{this.v_point[index]=value;}
	}
}

public class Particle
{
	public int index=0;
	public float mass=1.0f;
	public Complexor velocity,displacement,f_ext;
	public Complexor rigid_force,resistance;
	public VPoint rigid_nuclear;
    public bool isfix = false; 

	public Particle()
	{
		velocity = new Complexor ();
		displacement = new Complexor ();
		f_ext = new Complexor ();
		rigid_force = new Complexor ();
		resistance = new Complexor ();
		rigid_nuclear = new VPoint ();
	}
	public void SetIndex(int i)
	{
		index = i;
	}
	public void SetF(Complexor f)
	{
		f_ext=f;
	}
	public void SetVelocity(Complexor v)
	{
		velocity=v;
	}
	public void SetDisplacement(Complexor d)
	{
		displacement=d;
	}
}

public class SpringDamper
{
	public float origin_length;
	public Particle p1,p2;
	public Complexor change_length,force;
	public SpringDamper()
	{
		p1 = new Particle ();
		p2 = new Particle ();
		change_length = new Complexor ();
		force = new Complexor ();
	}
	public static bool operator==(SpringDamper s1,SpringDamper s2)
	{
		if((s1.p1.index==s2.p1.index&&s1.p2.index==s2.p2.index)||(s1.p2.index==s2.p1.index&&s1.p1.index==s2.p2.index)) 
			return true;
		else return false;
	}
	public static bool operator!=(SpringDamper s1,SpringDamper s2)
	{
		if(s1==s2) 
			return false;
		else return true;
	}
}

public class MassSpring_
{
	public Mesh mesh;
	public List<Particle> vector_particles=new List<Particle>();
	public List<int> level=new List<int>();
	public List<int> update_field = new List<int> ();
	public List<List<int>> near_par=new List<List<int>>();
	public List<List<SpringDamper>> near_spr=new List<List<SpringDamper>>();
	public float time_step;
	public int times;

	public List<int> indexs_select=new List<int>();
	/////力学参数
	public float ks,Cd,coefficient_spring,coefficient_damping;

	public MassSpring_()
	{
		time_step=0.01f;//0.016
		times=1;
		mesh = null;
		
		coefficient_spring=200;//200
		coefficient_damping=500;//500
		ks=3f;//1//3
		Cd=500;//120//1200
		
		//threshold_force=28;
		//depth_step=0.1;
		//形变中质点弹簧容器的容量
		//vector_particles.reserve(3000);
		//level.reserve(10);
		//update_field.reserve(1000);
		//near_par.reserve(10);
		//near_spr.reserve(10);
	}
	//////形变相关函数
	public void DeformationCalculate()
	{
		//float size_dis;
		Vector3[] tt=mesh.vertices;////
		Vector3 t=new Vector3();
		for(int i=0;i<times;i++)
		{
			for(int j=0;j<vector_particles.Count;j++)
			{
				if(vector_particles[j].velocity.GetSize()!=0&&!vector_particles[j].isfix)
				{
					vector_particles[j].displacement=time_step*vector_particles[j].velocity;
					t[0]=vector_particles[j].displacement[0]*vector_particles[j].displacement.GetSize();
					t[1]=vector_particles[j].displacement[1]*vector_particles[j].displacement.GetSize();
					t[2]=vector_particles[j].displacement[2]*vector_particles[j].displacement.GetSize();
					tt[vector_particles[j].index]+=t;
				}
			}
			mesh.vertices = tt;
			mesh.RecalculateBounds();
			for(int j=0;j<vector_particles.Count;j++)
			{
                if (!vector_particles[j].isfix)
                {
                    Complexor k1 = new Complexor(), k2 = new Complexor(), f_merge1 = new Complexor();
                    if (vector_particles[j].displacement.GetSize() != 0)
                    {
                        CalculateRigidForceAndResistance(vector_particles[j]);
                    }
                    f_merge1 = f_merge1 + vector_particles[j].f_ext + vector_particles[j].rigid_force + vector_particles[j].resistance;
                    for (int x = 0; x < near_spr[j].Count; x++)
                    {
                        Vector3 ts = new Vector3();
                        ts = mesh.vertices[near_spr[j][x].p1.index] - mesh.vertices[near_spr[j][x].p2.index];
                        near_spr[j][x].change_length.SetCpr(ts[0], ts[1], ts[2]);
                        CalculateForce(near_spr[j][x]);
                        f_merge1 = f_merge1 - near_spr[j][x].force;
                    }
                    float coe;
                    coe = time_step / vector_particles[j].mass;
                    k1 = coe * f_merge1;
                    vector_particles[j].velocity = vector_particles[j].velocity + k1;
                    if (vector_particles[j].velocity.GetSize() <= 0.0003005) vector_particles[j].velocity = k2;//0.0001
                    if (vector_particles[j].velocity.GetSize() >= 0.4) vector_particles[j].velocity.SetCpr(0.4f, vector_particles[j].velocity.GetDir());
                }
            }
		}
	}
	public void SetExternalForce(int index,Complexor f)
	{
		//indexs_select.Add (index);
		for(int i=0;i<vector_particles.Count;i++)
		{
            if (vector_particles[i].index == index)
                //vector_particles[i].f_ext = f;
                vector_particles[i].f_ext.SetCpr(f.GetSize(),f.GetDir());
                //vector_particles[i].f_ext.SetCpr(f[0] * f.GetSize(), f[1] * f.GetSize(), f[2] * f.GetSize());
		}
	}
	public void  DeleteExternalForce()
	{
        //for (int i = 0; i < indexs_select.Count; i++)
        //{
        //    vector_particles [indexs_select[i]].f_ext = new Complexor (0, 0, 0);
        //}
        //indexs_select.Clear();
        for (int i = 0; i < vector_particles.Count; i++)
        {
           // if (vector_particles[i].index == index)
            vector_particles[i].f_ext.SetCpr(0.0f,0.0f,0.0f);
        }
	}
	public void SetUpdateField()
	{
		//update_field=
	}
	public void BoundData(Mesh m)
	{
		Release ();
		mesh = m;
		InitMassAndSpring ();
	}
	public void Release()
	{
		vector_particles.Clear();
		//vector_spring.clear();
		update_field.Clear();
		near_par.Clear();
		near_spr.Clear();
		level.Clear();
	}
	public void te(Vector3 []t)
	{
		Vector3 []test=mesh.vertices;
		for (int j=0; j<10; j++) {
						for (int i=0; i<vector_particles.Count; i++) {
								test [i] += Vector3.up * Time.deltaTime*0.01f;
						}
			//Debug.Log("tttt:"+ Time.deltaTime);
						mesh.vertices = test;
						mesh.RecalculateBounds ();
				}
	}
	private void SearchPartice(int index)
	{
		List<int> level_=new List<int>();
		int ite,count;
		level.Clear();
		for(int i=0;i!=mesh.triangles.Length/3;i++)
		{
			ite=-1;
			count=3;
			for(int j=0;j<count;j++)
			{
				if(index==mesh.triangles[3*i+j])
					ite=j;
			}
			if(ite!=-1)
			{
				if(ite>=2) 
				{
					level_.Add(mesh.triangles[3*i+ite-2]);
				}
				if(ite>=1) 
				{
					level_.Add(mesh.triangles[3*i+ite-1]);
				}
				if((count-ite)>1) 
				{
					level_.Add(mesh.triangles[3*i+ite+1]);
				}
				if((count-ite)>2) 
				{
					level_.Add(mesh.triangles[3*i+ite+2]);
				}
			}
		}

		level_.Sort ();
		//Debug.Log ("aa:" + level_.Count);
		if (level_ [0] != level_ [1]) 
		{
			level.Add(level_[0]);
		}
		for (int i=1; i<level_.Count; i++)
		{
			if(level_[i]!=level_[i-1])
				level.Add(level_[i]);
		}
	}
	private void InitMassAndSpring()
	{
		bool flag=true;
		for(int i=0;i<mesh.vertices.Length;i++)
		{
			Particle temp=new Particle();
			temp.index=i;
			//temp.rigid_nuclear=mesh.vertices[i];
			temp.rigid_nuclear[0]=mesh.vertices[i][0];
			temp.rigid_nuclear[1]=mesh.vertices[i][1];
			temp.rigid_nuclear[2]=mesh.vertices[i][2];
			vector_particles.Add(temp);
		}
		for(int i=0;i<vector_particles.Count;i++)
		{

			List<SpringDamper> temp_sd=new List<SpringDamper>();
			SearchPartice(vector_particles[i].index);
			near_par.Add(level);
            if(level.Count<=4)
            {
                vector_particles[i].isfix = true;
            }
			for(int x=0;x!=level.Count;x++)
			{
				SpringDamper sd=new SpringDamper();
				sd.p1.index=i;
				sd.p2.index=level[x];
				sd.origin_length=Vector3.Distance(mesh.vertices[sd.p1.index],mesh.vertices[sd.p2.index]);
				temp_sd.Add(sd);
			}
			near_spr.Add(temp_sd);
			//Debug.Log ("dd:" + near_spr[i].Count);
			//temp_sd.Clear();
		}
	}
	private void CalculateRigidForceAndResistance(Particle p)
	{
		Complexor dir=new Complexor();
		VPoint temp_vp = new VPoint ();
		temp_vp.SetPoint (mesh.vertices[p.index][0],mesh.vertices[p.index][1],mesh.vertices[p.index][2]);
		dir=p.rigid_nuclear-temp_vp;
		p.rigid_force.SetCpr(ks*dir.GetSize(),dir.GetDir());
		p.resistance.SetCpr(Cd*(float)Math.Pow(p.velocity.GetSize(),2),p.velocity.GetDir());
		//Debug.Log ("vv_2:" + p.velocity [0]);
		p.resistance.Reverse();
		//Debug.Log ("vv_2_1:" + p.velocity [0]);
	}
	private void CalculateForce(SpringDamper sd)
	{
		Complexor spring_f=new Complexor(),damping_f=new Complexor();
		float size_dam;
		spring_f.SetCpr(coefficient_spring*(sd.change_length.GetSize()-sd.origin_length),sd.change_length.GetDir());
		size_dam=coefficient_damping*(float)Math.Sqrt(Math.Pow((sd.p1.velocity-sd.p2.velocity)*sd.change_length.unitization(),2));
		if((sd.p1.velocity-sd.p2.velocity)*sd.change_length.unitization()>=0)
		{
			damping_f.SetCpr(size_dam,sd.change_length.GetDir());
		}
		else
		{
			damping_f.SetCpr(size_dam,sd.change_length.GetDir());
			damping_f.Reverse();
		}
		sd.force=spring_f+damping_f;
	}
}

public class MassSpring : MonoBehaviour {

    //Mutex
    private static Mutex mut = new Mutex();

	MassSpring_ ms=new MassSpring_();
	Mesh mesh;
	//Complexor f;
    public float yy;
	Complexor f = new Complexor (0.0f, 0.0f, 0.0f);

    int ii = 0;
	private GameObject obj1;

	// Use this for initialization
	void Start () {

        obj1 = GameObject.Find("SUR_TOOL");

		mesh = GetComponent<MeshFilter>().mesh;
		ms.BoundData (mesh);

	}
	//hujiaqing
	// Update is called once per frame
	void Update () {

		//ms.mesh.vertices = vertices;
		//ms.mesh.RecalculateBounds();

        f.SetCpr(0.0f, yy, 0.0f);

        ms.SetExternalForce(60, f);
        //wait until it is safe to enter.

        //mut.WaitOne();
        ms.DeformationCalculate();

        Destroy(gameObject.GetComponent<MeshCollider>());
        gameObject.AddComponent<MeshCollider>();
        //ms.DeleteExternalForce();
        //Release the Mutex.
        //mut.ReleaseMutex();

       

	}
    void LateUpdate()
    {
        ii++;
        if (ii == 30)
        {
            ms.DeleteExternalForce();
            ii = 0;
        }
    }
	void OnGUI()
	{
		
	}
	void OnCollisionEnter(Collision collision) 
	{
        
	}
    void OnCollisionStay(Collision collision)
    {

        //wait until it is safe to enter.
        //mut.WaitOne();
        GEO_toolcontrol ct = obj1.GetComponent<GEO_toolcontrol>();

        Vector3 ve, ve_;
        ve = collision.contacts[0].point;
        ve_ = ct.GetForce()/10f;
        f.SetCpr(ve_[0], ve_[1], ve_[2]);//collision.relativeVelocity.magnitude 
        //f.SetCpr (0.0f, 0.0f, -0.1f);
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (Vector3.Distance(transform.TransformPoint(mesh.vertices[i]), ve) < 1.0f)
                ms.SetExternalForce(i, f);
        }

        //ms.DeformationCalculate();
        //Release the Mutex.
        //mut.ReleaseMutex();
    }
	void OnCollisionExit(Collision collision) 
	{
		//ms.DeleteExternalForce ();
	}
}
