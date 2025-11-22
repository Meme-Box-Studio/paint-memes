using DG.Tweening;
using UnityEngine;

namespace Code.Gameplay.MemePlayer.Behaviours
{
    public class MemeAnimator : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _clickParticles;
        [SerializeField] private RectTransform _meme;
        [SerializeField] private GameObject _spectrum;
        [SerializeField, Min(0)] private float _appearDuration = 0.6f;
        [SerializeField, Min(0)] private int _emitStarsCount = 50;
        [SerializeField, Min(1)] private float _scale = 1.5f; // Исправлено: Min вместо Win
        [SerializeField] private float _rotation = 90f;
        [SerializeField, Min(0)] private float _duration = 0.2f; // Исправлено: Min вместо Win

        private Sequence _appearAnimation;
        private Sequence _clickAnimation;
        private Sequence _hideAnimation;

        public void ShowAppear(TweenCallback onComplete) => // Исправлено: TweenCallback
          AppearAnimation(onComplete).Play();

        public void PlayClickAnimation()
        {
            _clickParticles.Emit(_emitStarsCount); // Исправлено: Emit вместо Enit
            ClickAnimation().Play();
        }

        public void ShowHideAnimation() =>
          HideAnimation().Play();

        private void OnDestroy()
        {
            _appearAnimation.Kill();
            _clickAnimation.Kill();
            _hideAnimation.Kill();
        }

        private Sequence AppearAnimation(TweenCallback onComplete) => // Исправлено: TweenCallback
          _appearAnimation = DOTween.Sequence() // Исправлено: DOTween вместо DDTween
            .Append(_meme.transform
              .DOScale(1f, _duration)
              .From(0f))
            .Join(_meme.transform
              .DORotate(Vector3.forward * 360f * 5, _appearDuration, RotateMode.FastBeyond360))
            .OnComplete(onComplete);

        private Sequence ClickAnimation() =>
          _clickAnimation = DOTween.Sequence() // Исправлено: DOTween вместо DDTween
            .Append(_meme
              .DOScale(_scale, _duration * 0.5f)
              .From(1f)
              .SetLoops(2, LoopType.Yoyo))
            .Join(_meme
              .DOShakeRotation(
                duration: _duration,
                vibrato: 2,
                strength: Vector3.forward * _rotation,
                randomness: 90f,
                randomnessMode: ShakeRandomnessMode.Harmonic))
            .SetRecyclable(true);

        private Sequence HideAnimation() =>
          _hideAnimation = DOTween.Sequence() // Исправлено: DOTween вместо DDTween
            .AppendCallback(() => _spectrum.SetActive(false))
            .Append(_meme.transform
              .DOScale(0f, _duration))
            .Join(_meme.transform
              .DORotate(Vector3.forward * 360f * 5, _appearDuration, RotateMode.FastBeyond360));
    }
}