if (typeof __org_ironwasp_js__.msgs === 'undefined')
{
	__org_ironwasp_js__.msgs = [];
	__org_ironwasp_js__.window_id = (new Date()).getTime() + "." + Math.floor(Math.random()*10000000);
	__org_ironwasp_js__.xhr_count = 0;
	
	__org_ironwasp_js__.send_msg = function(){
		if (__org_ironwasp_js__.msgs.length > 0) {
			var xhr = new XMLHttpRequest();
			var async = true;
			for(var i in __org_ironwasp_js__.msgs){
				if (__org_ironwasp_js__.msgs[i].action === "PageUnloading") {
					async = false;
				}
			}
			
			if (xhr.__open) {
				xhr.__open("POST","/ironwasp/api/core/jstracer/log_msg",async);
			}
			else
			{
				xhr.open("POST","/ironwasp/api/core/jstracer/log_msg",async);
			}
			xhr.setRequestHeader("Content-type","application/json");
			xhr.setRequestHeader("X-Ironwasp-Api-Call","https://ironwasp.org");
			if (xhr.__send) {
				xhr.__send(JSON.stringify(__org_ironwasp_js__.msgs));
			}
			else
			{
				xhr.send(JSON.stringify(__org_ironwasp_js__.msgs));
			}
			__org_ironwasp_js__.msgs = [];
		}
	}
	
	setInterval(__org_ironwasp_js__.send_msg, 500);
	
	window.addEventListener("load", function(){
			__org_ironwasp_js__.log({action:"PageLoaded", value:location.href});
		}, false);
	
	window.addEventListener("unload", function(){
			__org_ironwasp_js__.log({action:"PageUnloading", value:location.href});
			__org_ironwasp_js__.send_msg();
		}, false);
	
	if (__org_ironwasp_js__.config.eval) {
		__org_ironwasp_js__.eval = eval;
		eval = function(){
			if (typeof arguments[0] == 'string') {
				__org_ironwasp_js__.log({action:"EvalCalled", value:{args: arguments}});
			}
			return __org_ironwasp_js__.eval.apply(this, arguments);
		};
	}
	
	if (__org_ironwasp_js__.config.Function) {
		__org_ironwasp_js__.Function = Function;
		Function = function(){
			if (typeof arguments[1] == 'string') {
				__org_ironwasp_js__.log({action:"FunctionCalled", value:{args: arguments}});
			}
			return __org_ironwasp_js__.eval.Function(this, arguments);
		};
	}
	
	if (__org_ironwasp_js__.config.setTimeout) {
		__org_ironwasp_js__.setTimeout = setTimeout;
		setTimeout = function(){
			if (typeof arguments[0] == 'string') {
				__org_ironwasp_js__.log({action:"SetTimeoutCalled", value:{args: arguments}});
			}
			return __org_ironwasp_js__.setTimeout.apply(this, arguments);
		};
	}
	
	if (__org_ironwasp_js__.config.setInterval) {
		__org_ironwasp_js__.setInterval = setInterval;
		setInterval = function(){
			if (typeof arguments[0] == 'string') {
				__org_ironwasp_js__.log({action:"SetIntervalCalled", value:{args: arguments}});
			}
			return __org_ironwasp_js__.setInterval.apply(this, arguments);
		};
	}
	
	if (__org_ironwasp_js__.config.XHR){
		XMLHttpRequest.prototype.__open = XMLHttpRequest.prototype.open;
		XMLHttpRequest.prototype.open = function(){
			__org_ironwasp_js__.xhr_count++;
			this.xhr_id = __org_ironwasp_js__.xhr_count;
			__org_ironwasp_js__.log({action:"XhrOpenCalled", value:{args: arguments, xhr_id: __org_ironwasp_js__.window_id + "-" + this.xhr_id}});
			return XMLHttpRequest.prototype.__open.apply(this, arguments);
		}
		
		XMLHttpRequest.prototype.__send = XMLHttpRequest.prototype.send;
		XMLHttpRequest.prototype.send = function(){
			//__org_ironwasp_js__.log({action:"XhrSendCalled", value:{args: arguments}});
			this.setRequestHeader("x-org-ironwasp-js-trace-ajax", __org_ironwasp_js__.window_id + "-" + this.xhr_id);
			return XMLHttpRequest.prototype.__send.apply(this, arguments);
		}
	}
	
	__org_ironwasp_js__.log = function(msg){
		msg.url = location.href;
		msg.time = (new Date()).getTime();
		msg.window_id = __org_ironwasp_js__.window_id;
		__org_ironwasp_js__.msgs.push(msg);
	}
}
