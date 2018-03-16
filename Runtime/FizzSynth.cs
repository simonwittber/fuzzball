using UnityEngine;

namespace DifferentMethods.FuzzBall
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class FizzSynth : MonoBehaviour
    {
        public Synthesizer synthesizer;

        void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            synthesizer = ConstructRack();
        }

        public virtual Synthesizer ConstructRack()
        {
            throw new System.NotImplementedException();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            synthesizer.ReadAudio(data, channels);
        }
    }
}