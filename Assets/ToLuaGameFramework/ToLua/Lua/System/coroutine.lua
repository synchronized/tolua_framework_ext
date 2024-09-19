--------------------------------------------------------------------------------
--      Copyright (c) 2015 - 2016 , 蒙占志(topameng) topameng@gmail.com
--      All rights reserved.
--      Use, modification and distribution are subject to the "MIT License"
--------------------------------------------------------------------------------
local create = coroutine.create
local running = coroutine.running
local resume = coroutine.resume
local yield = coroutine.yield
local error = error
local select = select
local unpack = unpack or table.unpack
local tinsert = table.insert
local tremove = table.remove
local debug = debug
local FrameTimer = FrameTimer
local CoTimer = CoTimer

local comap = {}
local pool = {}
setmetatable(comap, {__mode = "kv"})

function coroutine.start(f, ...)
	local co = create(f)

	if running() == nil then
		local flag, msg = resume(co, ...)

		if not flag then
			error(debug.traceback(co, msg))
		end
	else
		local timer = nil
		local n = select("#", ...)
		local args = {...}

		local action = function()
			comap[co] = nil
            if timer then
			    timer.func = nil
                timer:Stop()
                tinsert(pool, timer)
            end
			local flag, msg = resume(co, unpack(args, 1, n))

			if not flag then
				error(debug.traceback(co, msg))
			end
		end

		if #pool > 0 then
			timer = tremove(pool)
			timer:Reset(action, 0, 1)
		else
			timer = FrameTimer.New(action, 0, 1)
		end

		comap[co] = timer
		timer:Start()
	end

	return co
end

function coroutine.wait(t, co, ...)
	co = co or running()
	local timer = nil
	local args = {...}
	local n = select("#", ...)

	local action = function()
		comap[co] = nil
        if timer then
            timer.func = nil
            timer:Stop()
        end
		local flag, msg = resume(co, unpack(args, 1, n))

		if not flag then
			error(debug.traceback(co, msg))
			return
		end
	end

	timer = CoTimer.New(action, t, 1)
	comap[co] = timer
	timer:Start()
	return yield()
end

function coroutine.step(t, co, ...)
	co = co or running()
	local timer = nil
	local args = {...}
	local n = select("#", ...)

	local action = function()
		comap[co] = nil
        if timer then
            timer.func = nil
			timer:Stop()
		    tinsert(pool, timer)
        end
		local flag, msg = resume(co, unpack(args, 1, n))

		if not flag then
			error(debug.traceback(co, msg))
			return
		end
	end

	if #pool > 0 then
		timer = tremove(pool)
		timer:Reset(action, t or 1, 1)
	else
		timer = FrameTimer.New(action, t or 1, 1)
	end

	comap[co] = timer
	timer:Start()
	return yield()
end

function coroutine.waitprogress(op, co)
	co = co or running()
	local timer = nil

	local action = function()
        local flag, msg
		if not op.IsDone then
		    flag, msg = resume(co, false, op.Progress)
        else
            comap[co] = nil
            if timer then
                timer:Stop()
                timer.func = nil
                tinsert(pool, timer)
            end
            flag, msg = resume(co, true, op.Error)
		end

		if not flag then
			error(debug.traceback(co, msg))
		end
	end

	if #pool > 0 then
		timer = tremove(pool)
		timer:Reset(action, 1, -1)
	else
		timer = FrameTimer.New(action, 1, -1)
	end
	comap[co] = timer
 	timer:Start()

    local _isDone = false
 	return function()
        if _isDone then
            error("async operation already done!")
        end
        local isDone, progress = yield()
        if isDone then
            _isDone = true
            local err = progress
            if err then
                error(err)
                return
            end
        end
        return isDone, progress
    end
end

-- return error
function coroutine.waitdone(op, co)
	co = co or running()
	local timer = nil

	local action = function()
		if op.IsDone then
            comap[co] = nil
            if timer then
                timer:Stop()
                timer.func = nil
                tinsert(pool, timer)
            end
            local flag, msg = resume(co)

            if not flag then
                error(debug.traceback(co, msg))
            end
		end
	end

	if #pool > 0 then
		timer = tremove(pool)
		timer:Reset(action, 1, -1)
	else
		timer = FrameTimer.New(action, 1, -1)
	end
	comap[co] = timer
 	timer:Start()

    yield()
end

function coroutine.stop(co)
 	local timer = comap[co]

 	if timer ~= nil then
 		comap[co] = nil
 		timer.func = nil
 		timer:Stop()
 	end
end
