
using UnityEngine;


[System.Serializable]
public class sounds
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public bool Music;
    public bool Sound;

}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public sounds[] soundarray;
    public float[] DefaultVolumes;

    public bool MusicOff;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        //  DontDestroyOnLoad(this.gameObject);
        DefaultVolumes = new float[soundarray.Length];
        for (int i = 0; i < soundarray.Length; i++)
        {
            soundarray[i].source = gameObject.AddComponent<AudioSource>();
            soundarray[i].source.volume = soundarray[i].volume;
            soundarray[i].source.clip = soundarray[i].clip;
            soundarray[i].source.pitch = soundarray[i].pitch;
            soundarray[i].source.loop = soundarray[i].loop;

            DefaultVolumes[i] = soundarray[i].volume;
        }

    }

    private void Start()
    {
        

       // playsound("mainmenu");

        //if (GameController.Instance.Mute)
        //{
        //    MusicOff = true;
        //    MuteAllSounds();
        //}
    }

    public void playsound(string name)
    {
        sounds s = null;


        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].name == name)
            {
                s = soundarray[i];
            }
        }


        if (s == null)
            return;
        else
        {
            Debug.Log(s.name);
            s.source.Play();
        }

    }




    public void playbuttonsound(string btnsound)
    {
        playsound(btnsound);
    }


    public void stopsound(string name)
    {
        sounds s = null;


        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].name == name)
            {
                s = soundarray[i];
            }
        }


        if (s == null)
            return;
        else
        {
            if (s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }

    public void stopallsounds()
    {
        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].source.isPlaying)
            {
                soundarray[i].source.Stop();
            }

        }
    }


    public void onlyplaycurrentsound(string name)
    {
        sounds s = null;


        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].name == name)
            {
                s = soundarray[i];
            }
            else
            {
                soundarray[i].source.Stop();

            }
        }


        if (s == null)
            return;
        else
        {
            s.source.Play();
        }
    }

    public void TurnSoundOnOff(bool turn)
    {
        for(int i = 0;i < soundarray.Length; i++)
        {
            if (soundarray[i].Sound)
            {
                Debug.Log("number of Audio Source   " + i + "   Bool " + turn);
                soundarray[i].source.mute = !turn;
            }
        }
    }
    public void TurnMusicOnOff(bool turn)
    {
        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].Music)
            {
                soundarray[i].source.mute = !turn;
            }
        }
    }
    public void MuteAllSounds()
    {
        for (int i = 0; i < soundarray.Length; i++)
        {
            soundarray[i].source.volume = 0f;
        }
    }
    //TO Set Music Value 
    public void SetMusicValue(float val)
    {
        for(int i = 0;i<soundarray.Length;i++)
        {
            if (soundarray[i].Music)
            {
                soundarray[i].volume = val;
                soundarray[i].source.volume = val;
            }
        }
    }
    public void SetSoundValue(float val)
    {
        for (int i = 0; i < soundarray.Length; i++)
        {
            if (soundarray[i].Sound)
            {
                soundarray[i].volume = val;
                soundarray[i].source.volume = val;
            }
        }
    }
}
