require 'spec_helper'
require 'rack-mini-profiler'
require 'mini_profiler/page_timer_struct'
require 'mini_profiler/storage/abstract_store'
require 'mini_profiler/storage/file_store'

describe Rack::MiniProfiler::FileStore do

  context 'page struct' do

    before do
      tmp = File.expand_path(__FILE__ + "/../../../tmp")
      Dir::mkdir(tmp) unless File.exists?(tmp)
      @store = Rack::MiniProfiler::FileStore.new(:path => tmp) 
    end

    describe 'storage' do

      it 'can store a PageStruct and retrieve it' do
        page_struct = Rack::MiniProfiler::PageTimerStruct.new({})
        page_struct['Id'] = "XYZ"
        page_struct['Random'] = "random"
        @store.save(page_struct)
        page_struct = @store.load("XYZ")
        page_struct['Random'].should == "random"
        page_struct['Id'].should == "XYZ"
      end    

      it 'can list unviewed items for a user' do 
        @store.set_unviewed('a', 'XYZ')
        @store.set_unviewed('a', 'ABC')
        @store.get_unviewed_ids('a').sort.to_a.should == ['XYZ', 'ABC'].sort.to_a
      end

      it 'can set an item to viewed once it is unviewed' do 
        @store.set_unviewed('a', 'XYZ')
        @store.set_unviewed('a', 'ABC')
        @store.set_viewed('a', 'XYZ')
        @store.get_unviewed_ids('a').should == ['ABC']
      end

    end

  end

end
