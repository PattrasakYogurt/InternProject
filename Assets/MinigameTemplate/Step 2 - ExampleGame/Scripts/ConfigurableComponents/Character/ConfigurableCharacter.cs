namespace MinigameTemplate.Example
{
    using UnityEngine;
    using UnityEngine.Events;

    public class ConfigurableCharacter : MonoBehaviour
    {
        [field: SerializeField] public Transform AssetHolder { get; private set; }
        [field: SerializeField, ShowOnly] private Animator animator;
        [field: SerializeField, ShowOnly] public Transform weaponHolderLeft { get; private set; }
        [field: SerializeField, ShowOnly] public Transform weaponHolderRight { get; private set; }

        [field: SerializeField, ShowOnly] public Transform weaponLeft { get; private set; }
        [field: SerializeField, ShowOnly] public Transform weaponRight { get; private set; }

        EffectSkinBlink effectSkinBlink;
        EffectSkinBlink EffectSkinBlink
        {
            get
            {
                if (effectSkinBlink == null) effectSkinBlink = GetComponent<EffectSkinBlink>();

                return effectSkinBlink;

            }
            set { effectSkinBlink = value; }
        }

        public Animator Animator
        {
            get
            {
                if (animator == null)
                {
                    if (AssetHolder.TryGetComponent<Animator>(out animator) == false)
                    {
                        Debug.LogError("New skin doesn't have animator");
                    }
                }
                //if(animator.enabled == false) animator.enabled = true;

                return animator;
            }
            private set { animator = value; }
        }
        public UnityEvent<GameObject> OnSkinChange { get; private set; } = new();//{ get; private set; } = new();

        [field: SerializeField, ShowOnly] private GameObject reviveVFX;
        [field: SerializeField, ShowOnly] private GameObject deathVFX;


        public void SetupHandWeapon(GameObject weapon, bool left)
        {
            if (AssetHolder.childCount > 0)
            {
                var weaponHolder = weaponHolderLeft;

                if (left == false)
                {
                    weaponHolder = weaponHolderRight;
                }

                if (weaponHolder == null) return;

                foreach (Transform currentTool in weaponHolder)
                    Destroy(currentTool.gameObject);

                if (left) weaponLeft = weapon.transform;
                else weaponRight = weapon.transform;

                if (weapon == null) return;

                weapon.transform.SetParent(weaponHolder);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = default;
                weapon.transform.localScale = Vector3.one;
            }
        }


        private Transform FindChildRecursive(Transform source, bool left)
        {

            string nameToLookFor = left ? "WeaponHolderLeft" : "WeaponHolderRight";
            if (source.gameObject.name == nameToLookFor)
            {
                return source;
            }

            foreach (Transform child in source)
            {
                Transform result = FindChildRecursive(child.transform, left);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public void SetupSkin(GameObject skin)
        {
            foreach (Transform t in AssetHolder)
            {
                Destroy(t.gameObject);
                break;
            }

            skin.transform.SetParent(AssetHolder);
            skin.transform.localPosition = Vector3.zero;
            skin.transform.localRotation = default;
            skin.transform.localScale = Vector3.one;

            if (skin.TryGetComponent<Animator>(out var skinAnimator))
            {
                Animator.runtimeAnimatorController = skinAnimator.runtimeAnimatorController;
                Animator.avatar = skinAnimator.avatar;
                Animator.Rebind();
                skinAnimator.enabled = false;
            }


            weaponHolderLeft = FindChildRecursive(AssetHolder, true);
            weaponHolderRight = FindChildRecursive(AssetHolder, false);


            var effectSkinBlink = EffectSkinBlink;

            if (effectSkinBlink) effectSkinBlink.Setup(skin);

            OnSkinChange.Invoke(skin);


        }

        public void SetupEffects(GameObject reviveVFX, GameObject deathVFX)
        {
            this.reviveVFX = reviveVFX;
            this.deathVFX = deathVFX;
        }

        public void DoReviveEffect()
        {
            var effect = Instantiate(reviveVFX);

            effect.transform.position = transform.position + Vector3.up * 1f;
        }
        public void DoDeathEffect()
        {
            var effect = Instantiate(deathVFX);

            effect.transform.position = transform.position + Vector3.up * 1f;
        }
    }

}