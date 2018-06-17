if (typeof __org_ironwasp_js__ !== "undefined" && __org_ironwasp_js__.config.innerHTML) {
	(function(){
		var b = document.getElementsByTagName('Body')[0];
		var config = { attributes: true, childList: true, subtree:true, attributeOldValue:true, characterData: false };
		var observer = new MutationObserver( function(mutations) {
			mutations.forEach(function(mutation) {
				if (mutation.type === "attributes") {
					if (mutation.target.attributes[mutation.attributeName]) {
							__org_ironwasp_js__.log({action: "AttributeChanged", value: {
							nodeName: mutation.target.tagName,
							attributeName: mutation.attributeName,
							attributeValue: mutation.target.attributes[mutation.attributeName].value}
						});
					}
				}
				else
				{
					var val = [];
					for(var i in mutation.addedNodes)
					{
						if (mutation.addedNodes[i].outerHTML) {
							val.push(mutation.addedNodes[i].outerHTML);
						}
					}
					if (val.length > 0) {
						__org_ironwasp_js__.log({action:"NodeAdded", value: val});
					}
				}
			});
		});
		observer.observe(b, config);
	})();
}