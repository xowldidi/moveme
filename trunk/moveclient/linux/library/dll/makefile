LIBMOVE = libmove.so

$(LIBMOVE): moveclient.o
	ld -shared -soname libmove.so.1 -o libmove.so.1.0 -lc moveclient.o -lpthread
	/sbin/ldconfig -v -n .
	ln -sf libmove.so.1 libmove.so

moveclient.o: moveclient.c
	$(CC) -fPIC -c moveclient.c
		
clean:
	$(RM) *.o $(LIBMOVE)*
