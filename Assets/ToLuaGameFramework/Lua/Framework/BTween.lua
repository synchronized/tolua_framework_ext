BTween = {}

--开始模拟抛物线动画（height:距离初始点的高度  duration：0-1的分段数）-----------------------------------------------------------------------------
function BTween.StartParabola(delay, trans, toPos, height, delta, elasticity, onCollision, onStop, worldSpace)
    ToLuaGameFramework.BTween.StartParabola(delay, trans, toPos, height, delta, worldSpace, elasticity, nil, onCollision, nil, onStop)
end

--终止抛物线动画
function BTween.StopParabola(trans)
    ToLuaGameFramework.BTween.StopParabola(trans)
end

--开始对单个数值进行Tween运算(onUpdate:返回实时数值 onFinish：不返回值)------------------------------------------------------------------------------
function BTween.Value(delay, startValue, toValue, duration, easeType, onUpdate, onFinish)
    ToLuaGameFramework.BTween.Value(delay, startValue, toValue, duration, easeType, onUpdate, onFinish)
end


Ease = {}

--引用一遍以支持IDE语法提示
Ease.Unset = DG.Tweening.Ease.Unset
Ease.Linear = DG.Tweening.Ease.Linear
Ease.InSine = DG.Tweening.Ease.InSine
Ease.OutSine = DG.Tweening.Ease.OutSine
Ease.InOutSine = DG.Tweening.Ease.InOutSine
Ease.InQuad = DG.Tweening.Ease.InQuad
Ease.OutQuad = DG.Tweening.Ease.OutQuad
Ease.InOutQuad = DG.Tweening.Ease.InOutQuad
Ease.InCubic = DG.Tweening.Ease.InCubic
Ease.OutCubic = DG.Tweening.Ease.OutCubic
Ease.InOutCubic = DG.Tweening.Ease.InOutCubic
Ease.InQuart = DG.Tweening.Ease.InQuart
Ease.OutQuart = DG.Tweening.Ease.OutQuart
Ease.InOutQuart = DG.Tweening.Ease.InOutQuart
Ease.InQuint = DG.Tweening.Ease.InQuint
Ease.OutQuint = DG.Tweening.Ease.OutQuint
Ease.InOutQuint = DG.Tweening.Ease.InOutQuint
Ease.InExpo = DG.Tweening.Ease.InExpo
Ease.OutExpo = DG.Tweening.Ease.OutExpo
Ease.InOutExpo = DG.Tweening.Ease.InOutExpo
Ease.InCirc = DG.Tweening.Ease.InCirc
Ease.OutCirc = DG.Tweening.Ease.OutCirc
Ease.InOutCirc = DG.Tweening.Ease.InOutCirc
Ease.InElastic = DG.Tweening.Ease.InElastic
Ease.OutElastic = DG.Tweening.Ease.OutElastic
Ease.InOutElastic = DG.Tweening.Ease.InOutElastic
Ease.InBack = DG.Tweening.Ease.InBack
Ease.OutBack = DG.Tweening.Ease.OutBack
Ease.InOutBack = DG.Tweening.Ease.InOutBack
Ease.InBounce = DG.Tweening.Ease.InBounce
Ease.OutBounce = DG.Tweening.Ease.OutBounce
Ease.InOutBounce = DG.Tweening.Ease.InOutBounce
Ease.Flash = DG.Tweening.Ease.Flash
Ease.InFlash = DG.Tweening.Ease.InFlash
Ease.OutFlash = DG.Tweening.Ease.OutFlash
Ease.InOutFlash = DG.Tweening.Ease.InOutFlash
