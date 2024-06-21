
// 确保所有必要的类和方法在同一个文件中，因为它们在JavaScript中是相互依赖的。
using System;
using System.Collections.Generic;
using UnityEngine;

// 假设Sprite、ImageManager、ScreenSprite和AudioManager类已经存在
public partial class Sprite_AnimationMV : Sprite
{
    // 初始化成员变量
    private List<SpriteRenderer> _targets = new List<SpriteRenderer>();
    private Animation _animation;
    private bool _mirror;
    private float _delay;
    private float _rate;
    private float _duration;
    private Color _flashColor;
    private float _flashDuration;
    private float _screenFlashDuration;
    private float _hidingDuration;
    private float _hue1;
    private float _hue2;
    private Texture2D _bitmap1;
    private Texture2D _bitmap2;
    private Sprite[] _cellSprites;
    private ScreenSprite _screenFlashSprite;
    private int _currentFrameIndex;

    public Sprite_AnimationMV()
    {
        Initialize();
    }

    public override void Initialize(params object[] args)
    {
        base.Initialize(args);
        InitMembers();
    }

    private void InitMembers()
    {
        _currentFrameIndex = -1;
        this.z = 8;
    }

    public void Setup(List<SpriteRenderer> targets, Animation animation, bool mirror, float delay)
    {
        _targets = targets;
        _animation = animation;
        _mirror = mirror;
        _delay = delay;
        if (_animation != null)
        {
            SetupRate();
            SetupDuration();
            LoadBitmaps();
            CreateCellSprites();
            CreateScreenFlashSprite();
        }
    }

    private void SetupRate()
    {
        _rate = 4;
    }

    private void SetupDuration()
    {
        _duration = _animation.Frames.Length * _rate + 1;
    }

    public override void Update()
    {
        base.Update();
        UpdateMain();
        UpdateFlash();
        UpdateScreenFlash();
        UpdateHiding();
    }

    private void UpdateFlash()
    {
        if (_flashDuration > 0)
        {
            _flashDuration--;
            Color blendedColor = _flashColor;
            blendedColor.a *= (_flashDuration - 1) / _flashDuration;
            foreach (var target in _targets)
            {
                target.SetBlendColor(blendedColor);
            }
        }
    }

    private void UpdateScreenFlash()
    {
        if (_screenFlashDuration > 0)
        {
            _screenFlashDuration--;
            if (_screenFlashSprite != null)
            {
                _screenFlashSprite.X = -AbsoluteX();
                _screenFlashSprite.Y = -AbsoluteY();
                _screenFlashSprite.Opacity *= (_screenFlashDuration - 1) / _screenFlashDuration;
                _screenFlashSprite.Visible = _screenFlashDuration > 0;
            }
        }
    }

    private float AbsoluteX()
    {
        float x = 0;
        Transform current = this.transform;
        while (current != null)
        {
            x += current.position.x;
            current = current.parent?.transform;
        }
        return x;
    }

    private float AbsoluteY()
    {
        float y = 0;
        Transform current = this.transform;
        while (current != null)
        {
            y += current.position.y;
            current = current.parent?.transform;
        }
        return y;
    }

    private void UpdateHiding()
    {
        if (_hidingDuration > 0)
        {
            _hidingDuration--;
            if (_hidingDuration == 0)
            {
                foreach (var target in _targets)
                {
                    target.Show();
                }
            }
        }
    }

    public bool IsPlaying()
    {
        return _duration > 0;
    }

    private void LoadBitmaps()
    {
        string name1 = _animation.Animation1Name;
        string name2 = _animation.Animation2Name;
        _hue1 = _animation.Animation1Hue;
        _hue2 = _animation.Animation2Hue;
        _bitmap1 = ImageManager.LoadAnimation(name1);
        _bitmap2 = ImageManager.LoadAnimation(name2);
    }

    public bool IsReady()
    {
        return (_bitmap1 != null && _bitmap1.IsReady()) && (_bitmap2 != null && _bitmap2.IsReady());
    }

    private void CreateCellSprites()
    {
        _cellSprites = new Sprite[16];
        for (int i = 0; i < 16; i++)
        {
            Sprite sprite = new Sprite();
            sprite.Anchor = new Vector2(0.5f, 0.5f);
            _cellSprites[i] = sprite;
            this.AddChild(sprite);
        }
    }

    private void CreateScreenFlashSprite()
    {
        _screenFlashSprite = new ScreenSprite();
        this.AddChild(_screenFlashSprite);
    }

    private void UpdateMain()
    {
        if (IsPlaying() && IsReady())
        {
            if (_delay > 0)
            {
                _delay--;
            }
            else
            {
                _duration--;
                UpdatePosition();
                if (_duration % _rate == 0)
                {
                    UpdateFrame();
                }
                if (_duration <= 0)
                {
                    OnEnd();
                }
            }
        }
    }

    private void UpdatePosition()
    {
        if (_animation.Position == 3)
        {
            transform.position = new Vector3(transform.parent.width / 2f, transform.parent.height / 2f, 0);
        }
        else if (_targets.Count > 0)
        {
            SpriteRenderer target = _targets[0];
            RectTransform parent = target.GetComponent<RectTransform>();
            RectTransform grandparent = parent != null ? parent.parent : null;
            transform.position = new Vector3(target.position.x, target.position.y, 0);
            if (parent == grandparent)
            {
                transform.position += parent.position;
            }
            if (_animation.Position == 0)
            {
                transform.position.y -= target.rect.height;
            }
            else if (_animation.Position == 1)
            {
                transform.position.y -= target.rect.height / 2f;
            }
        }
    }

    private void UpdateFrame()
    {
        if (_duration > 0)
        {
            _currentFrameIndex = _animation.Frames.Length - Mathf.FloorToInt((_duration + _rate - 1) / _rate);
            UpdateAllCellSprites(_animation.Frames[_currentFrameIndex]);
            foreach (Timing timing in _animation.Timings)
            {
                if (timing.Frame == _currentFrameIndex)
                {
                    ProcessTimingData(timing);
                }
            }
        }
    }

    private void UpdateAllCellSprites(Texture2D frame)
    {
        if (_targets.Count > 0)
        {
            for (int i = 0; i < _cellSprites.Length; i++)
            {
                Sprite sprite = _cellSprites[i];
                if (i < frame.GetPixels32().Length)
                {
                    UpdateCellSprite(sprite, frame.GetPixels32()[i]);
                }
                else
                {
                    sprite.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateCellSprite(Sprite sprite, Color32 cell)
    {
        int pattern = cell.r / 192;
        int cellX = (pattern % 5) * 192;
        int cellY = Mathf.FloorToInt((pattern % 100) / 5) * 192;
        bool mirror = _mirror;
        sprite.texture = pattern < 100 ? _bitmap1 : _bitmap2;
        sprite.SetHue(pattern < 100 ? _hue1 : _hue2);
        sprite.SetFrame(cellX, cellY, 192, 192);
        sprite.position = new Vector3(cell.g, cell.b, 0);
        sprite.rotation = cell.a * Mathf.PI / 180;
        sprite.localScale = new Vector3(cell.r / 100, cell.r / 100, 1);
        sprite.opacity = cell.a;
        sprite.BlendMode = (UnityEngine.Rendering.BlendMode)cell.z;

        if (cell.w > 0)
        {
            sprite.localScale.x *= -1;
        }
        if (mirror)
        {
            sprite.position *= -1;
            sprite.rotation *= -1;
            sprite.localScale.x *= -1;
        }
    }

    private void ProcessTimingData(Timing timing)
    {
        float flashDuration = timing.FlashDuration * _rate;
        switch (timing.FlashScope)
        {
            case 1:
                StartFlash(new Color4(timing.FlashColor.r, timing.FlashColor.g, timing.FlashColor.b, timing.FlashColor.a), flashDuration);
                break;
            case 2:
                StartScreenFlash(new Color4(timing.FlashColor.r, timing.FlashColor.g, timing.FlashColor.b, timing.FlashColor.a), flashDuration);
                break;
            case 3:
                StartHiding(flashDuration);
                break;
        }
        if (timing.Se != null)
        {
            AudioManager.PlaySe(timing.Se);
        }
    }

    private void StartFlash(Color4 color, float duration)
    {
        _flashColor = color;
        _flashDuration = duration;
    }

    private void StartScreenFlash(Color4 color, float duration)
    {
        _screenFlashDuration = duration;
        if (_screenFlashSprite != null)
        {
            _screen